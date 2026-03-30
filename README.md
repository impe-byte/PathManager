# PathManager Professional Suite

## Descrizione
PathManager Professional è lo standard di efficienza industriale per l'identificazione, l'analisi e la risoluzione automatizzata massiva per la violazione dei limiti legacy dei path di Windows (MAX_PATH = 260 caratteri). Studiata appositamente per IT Manager e Sysadmin responsabili della migrazione dati.

## Feature Architetturali (Release 1.0.0)
- **Architettura Clean "Zero I/O Core":** Separazione completa tra la logica di calcolo dei percorsi stringati ("Domain" e "Engine") ed il File System sottostante assecondando una strategia agnostica per il testing intensivo.
- **Bypass del limite `MAX_PATH`:** Piena integrazione asincrona e nativa via P/Invoke (`kernel32.dll` - `MoveFileW`), mascherando dinamicamente path UNC e locali col flag `\\?\`. Nessuna eccezione causata dalla limitazione legacy!
- **Interfaccia Utente Asincrona con Dry-Run (Preview):** Motore di esecuzione puramente non bloccante. Generazione di "Transaction Plan" pre-calcolati esaminabili visivamente prima del "commit", applicando appieno i pattern *TDD & Clean Code*.
- **Sistema di Audit & Reporting (CSV):** Tracciabilità assoluta per sysadmins tramite log CSV post-transazionali per verificare l'esito reale (Successo, Fallito) e l'originale mappatura per ogni file.
