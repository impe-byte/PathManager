#include <iostream>
#include <filesystem>
#include <string>
#include <vector>
#include <fstream>
#include <iomanip>
#include <chrono>

namespace fs = std::filesystem;

// Struttura per immagazzinare i dati di un path che supera la soglia
struct OverThresholdPath {
    std::string relativePath;
    int charCount;
    int excessChars;
};

// Struttura per il report riassuntivo
struct ScanReport {
    unsigned long long totalFiles = 0;
    unsigned long long totalFolders = 0;
    unsigned long long totalSizeBytes = 0;
    int thresholdLimit = 0;
    std::vector<OverThresholdPath> badPaths;
};

// Funzione helper per formattare i byte in un formato leggibile
std::string formatSize(unsigned long long bytes) {
    const char* suffixes[] = {"B", "KB", "MB", "GB", "TB", "PB"};
    int s = 0;
    double count = static_cast<double>(bytes);
    while (count >= 1024 && s < 5) {
        s++;
        count /= 1024;
    }
    std::ostringstream ss;
    ss << std::fixed << std::setprecision(2) << count << " " << suffixes[s];
    return ss.str();
}

// Funzione helper per aggiungere il prefisso UNC o Win32 per sbloccare il limite di MAX_PATH (260)
std::wstring getLongPath(const std::wstring& path) {
    if (path.length() >= 4 && path.substr(0, 4) == L"\\\\?\\") {
        return path;
    }
    // Gestione dei percorsi UNC (es. \\server\share -> \\?\UNC\server\share)
    if (path.length() >= 2 && path.substr(0, 2) == L"\\\\") {
        return L"\\\\?\\UNC\\" + path.substr(2);
    }
    // Gestione percorsi locali standard (es. C:\folder -> \\?\C:\folder)
    return L"\\\\?\\" + path;
}

// Funzione helper per rimuovere il prefisso lungo quando visualizziamo o contiamo la stringa
std::string removeLongPathPrefix(std::string pathStr) {
    if (pathStr.length() >= 4 && pathStr.substr(0, 4) == "\\\\?\\") {
        if (pathStr.length() >= 8 && pathStr.substr(0, 8) == "\\\\?\\UNC\\") {
            return "\\\\" + pathStr.substr(8);
        } else {
            return pathStr.substr(4);
        }
    }
    return pathStr;
}

int main() {
    std::cout << "--- PathManager Scanner ---\n\n";

    std::string rootInput;
    std::cout << "Inserisci il percorso radice (es. C:\\Cartella o \\\\Server\\Condivisione): ";
    std::getline(std::cin, rootInput);

    // Rimuoviamo eventuali doppi apici (es. da copia-incolla del terminale Windows)
    if (!rootInput.empty() && rootInput.front() == '"' && rootInput.back() == '"') {
        rootInput = rootInput.substr(1, rootInput.length() - 2);
    }

    if (rootInput.empty()) {
        std::cerr << "Percorso vuoto!\n";
        return 1;
    }

    int threshold = 0;
    std::cout << "Inserisci la soglia di caratteri (es. 150): ";
    if (!(std::cin >> threshold) || threshold <= 0) {
        std::cerr << "Soglia non valida!\n";
        return 1;
    }

    std::error_code ec;
    fs::path rootPath = std::filesystem::path(rootInput);

    // Risolviamo il percorso assoluto per evitare problemi con path relativi e limiti
    fs::path absoluteRoot = fs::absolute(rootPath, ec);
    if (ec) {
        std::cerr << "Errore nella risoluzione del percorso: " << ec.message() << "\n";
        return 1;
    }

    if (!fs::exists(absoluteRoot, ec)) {
        std::cerr << "Il percorso specificato non esiste o non e' accessibile!\n";
        return 1;
    }

    // Risolviamo la lunghezza base per il calcolo
    std::string absRootStr = removeLongPathPrefix(absoluteRoot.string());
    int rootLength = static_cast<int>(absRootStr.length());

    // Se la root finisce con \ rimuoviamo dal conteggio base affinché venga processato correttamente 
    if (!absRootStr.empty() && (absRootStr.back() == '\\' || absRootStr.back() == '/')) {
        rootLength--;
    }

    // Usiamo il workaround API \\?\ tramite wstring per sbloccare il limite di 32.767
    std::wstring longRootWStr = getLongPath(absoluteRoot.wstring());
    fs::path longRootPath(longRootWStr);

    ScanReport report;
    report.thresholdLimit = threshold;

    std::cout << "\nInizio scansione (ignoro Access Denied autom.)...\n";
    auto startTime = std::chrono::high_resolution_clock::now();

    // skip_permission_denied ci permette di proseguire anche quando non abbiamo permessi
    fs::directory_options options = fs::directory_options::skip_permission_denied;

    auto it = fs::recursive_directory_iterator(longRootPath, options, ec);
    auto end = fs::recursive_directory_iterator();

    if (ec) {
        std::cerr << "Impossibile aprire la directory radice: " << ec.message() << "\n";
        return 1;
    }

    while (it != end) {
        const auto& entry = *it;
        
        bool isSymlink = entry.is_symlink(ec);
        if (!ec && isSymlink) {
            // Ignoriamo i symlink
            it.increment(ec);
            if (ec) ec.clear();
            continue;
        }
        ec.clear();

        bool isDir = entry.is_directory(ec);
        if (!ec && isDir) {
            report.totalFolders++;
        } else if (!ec && entry.is_regular_file(ec)) {
            report.totalFiles++;
            std::uintmax_t fsize = entry.file_size(ec);
            if (!ec) {
                report.totalSizeBytes += fsize;
            }
        }
        ec.clear();

        // Trasformiamo in stringa rimuovendo il prefisso interno
        std::string currentPathStr = removeLongPathPrefix(entry.path().string());
        int currentPathLength = static_cast<int>(currentPathStr.length());

        // Formula richiesta: Lunghezza Relativa = Lunghezza(Path_Trovato) - Lunghezza(Root) - 1
        int relativeLength = currentPathLength - rootLength - 1;

        if (relativeLength > threshold) {
            OverThresholdPath badPath;
            int startPos = rootLength;
            if (startPos < currentPathLength && (currentPathStr[startPos] == '\\' || currentPathStr[startPos] == '/')) {
                startPos++; // Saltiamo la backslash divisoria
            }

            if (startPos < currentPathLength) {
                badPath.relativePath = currentPathStr.substr(startPos);
            } else {
                badPath.relativePath = currentPathStr;
            }
            
            badPath.charCount = relativeLength;
            badPath.excessChars = relativeLength - threshold;
            report.badPaths.push_back(badPath);
        }

        it.increment(ec);
        if (ec) {
            // Se c'è un errore imprevisto (es. IO error) durante l'iterazione
            ec.clear(); // lo ignoriamo e passiamo avanti
        }
    }

    auto endTime = std::chrono::high_resolution_clock::now();
    std::chrono::duration<double> elapsed = endTime - startTime;

    std::cout << "Scansione terminata in " << elapsed.count() << " secondi!\n";
    std::cout << "Generazione report in corso...\n";

    // Salviamo Report.txt e Report.csv per massima compatibilità
    std::ofstream outFile("Report_PathManager.txt");
    if (!outFile) {
        std::cerr << "Errore durante la creazione del file di report txt.\n";
        return 1;
    }

    outFile << "[Sezione 1: Statistiche Globali]\n";
    outFile << "Root analizzata: " << rootInput << "\n";
    outFile << "Totale Cartelle: " << report.totalFolders << "\n";
    outFile << "Totale File: " << report.totalFiles << "\n";
    outFile << "Peso Totale: " << formatSize(report.totalSizeBytes) << "\n";
    outFile << "Soglia impostata: " << report.thresholdLimit << " caratteri\n";
    outFile << "File/Cartelle oltre la soglia: " << report.badPaths.size() << "\n\n";

    if (!report.badPaths.empty()) {
        outFile << "[Sezione 2: Dettaglio Soglia Violata]\n";
        for (const auto& bp : report.badPaths) {
            outFile << "[+" << bp.excessChars << " caratteri] (Tot: " << bp.charCount << ") - " << bp.relativePath << "\n";
        }
    }
    outFile.close();
    std::cout << "Report TXT generato: Report_PathManager.txt\n";

    std::ofstream csvFile("Report_PathManager.csv");
    if (csvFile) {
        // Aggiungiamo il BOM UTF-8 per Excel
        csvFile << "\xEF\xBB\xBF";
        csvFile << "Eccesso Caratteri,Caratteri Totali,Percorso Relativo\n";
        for (const auto& bp : report.badPaths) {
            csvFile << bp.excessChars << "," << bp.charCount << ",\"" << bp.relativePath << "\"\n";
        }
        csvFile.close();
        std::cout << "Report CSV generato: Report_PathManager.csv\n";
    }

    return 0;
}
