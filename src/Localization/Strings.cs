using System.Collections.Generic;

namespace PathManager.Localization
{
    public static class Strings
    {
        public static string CurrentLang = "IT";

        private static readonly Dictionary<string, Dictionary<string, string>> _dict = new Dictionary<string, Dictionary<string, string>>()
        {
            {
                "EN", new Dictionary<string, string>()
                {
                    { "Title", "Path Manager Professional" },
                    { "RootLabel", "Root Path to scan:" },
                    { "Browse", "Browse..." },
                    { "ThresholdLabel", "Max allowed characters limit:" },
                    { "ScanBtn", "Start Professional Scan" },
                    { "StatusReady", "Ready." },
                    { "StatusScanning", "Scanning Engine active... Please wait." },
                    { "StatusDone", "Scan completed successfully!" },
                    { "StatusError", "Error during scan: " },
                    { "SelectValidRoot", "Please select a valid root folder." },
                    { "OpenTxt", "Open TXT Report" },
                    { "OpenCsv", "Open CSV Report" },
                    { "OpenHtml", "Open HTML Report" },
                    { "StatsGlobal", "Global Statistics" },
                    { "StatsRoot", "Analyzed Root:" },
                    { "StatsFolders", "Total Folders:" },
                    { "StatsFiles", "Total Files:" },
                    { "StatsSize", "Total Size:" },
                    { "StatsThreshold", "Threshold Limit:" },
                    { "StatsViolations", "Paths Exceeding Threshold:" },
                    { "DetailViolations", "Threshold Violations Detail" },
                    { "ColExcess", "Excess" },
                    { "ColTotal", "Total" },
                    { "ColRelative", "Relative Path" },
                    { "LangToggle", "ENG" },
                    { "FileLabel", "files" }
                }
            },
            {
                "IT", new Dictionary<string, string>()
                {
                    { "Title", "Path Manager Professional" },
                    { "RootLabel", "Percorso Radice (Root):" },
                    { "Browse", "Sfoglia..." },
                    { "ThresholdLabel", "Soglia massima caratteri:" },
                    { "ScanBtn", "Avvia Scansione Completa" },
                    { "StatusReady", "Pronto all'uso." },
                    { "StatusScanning", "Scansione in corso... Attendere." },
                    { "StatusDone", "Scansione completata con successo!" },
                    { "StatusError", "Errore durante la scansione: " },
                    { "SelectValidRoot", "Seleziona una cartella radice valida e accessibile." },
                    { "OpenTxt", "Apri Report TXT" },
                    { "OpenCsv", "Apri Report CSV" },
                    { "OpenHtml", "Apri Report HTML" },
                    { "StatsGlobal", "Statistiche Globali" },
                    { "StatsRoot", "Root analizzata:" },
                    { "StatsFolders", "Totale Cartelle:" },
                    { "StatsFiles", "Totale File:" },
                    { "StatsSize", "Peso Totale:" },
                    { "StatsThreshold", "Soglia impostata:" },
                    { "StatsViolations", "File/Cartelle oltre la soglia:" },
                    { "DetailViolations", "Dettaglio Soglia Violata" },
                    { "ColExcess", "Eccesso" },
                    { "ColTotal", "Totale" },
                    { "ColRelative", "Percorso Relativo" },
                    { "LangToggle", "ITA" },
                    { "FileLabel", "file" }
                }
            }
        };

        public static string Get(string key)
        {
            if (_dict.ContainsKey(CurrentLang) && _dict[CurrentLang].ContainsKey(key))
                return _dict[CurrentLang][key];
            return key;
        }

        public static string FormatSize(ulong bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
            int s = 0;
            double count = bytes;
            while (count >= 1024 && s < 5)
            {
                s++;
                count /= 1024;
            }
            return string.Format("{0:0.00} {1}", count, suffixes[s]);
        }
    }
}
