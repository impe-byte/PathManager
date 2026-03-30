# PathManager Professional - Project Bible & Architecture

## 1. Vision e Obiettivo
- **Descrizione:** Suite professionale per l'analisi, la preview e la risoluzione massiva (modifica/rinomina) delle violazioni di limite lunghezza percorsi (MAX_PATH) su sistemi Windows.
- **Target Audience:** Sysadmin, IT Manager, Data Migrator (SharePoint/NAS).

## 2. Tech Stack & Infrastruttura
- **Gestione Repo:** Soluzione .NET / C# Moderna.
- **Client/Frontend:** [Valutare aggiornamento da WinForms a WPF o Avalonia UI per UI moderne, oppure mantenere WinForms ottimizzato].
- **Core Engine/Logic:** C# puro (Logica di business slegata dall'infrastruttura).
- **Infrastruttura (I/O):** Interfacce Win32 API native (`kernel32.dll` - `WIN32_FIND_DATAW`, `MoveFileW`) per bypassare limiti 260 char.
- **Testing:** xUnit / NUnit per copertura logica core e transazioni virtuali.

## 3. Direzione Artistica e UI/UX
- **Stile Visivo:** Interfaccia pulita, professionale, focalizzata sui dati. Colori di stato chiari (Rosso per violazioni, Verde per risoluzioni).
- **Regole UI:** La UI deve sempre essere asincrona e reattiva, implementando progress bar per operazioni massive. La Preview delle modifiche deve essere mostrata in una DataGrid chiara prima dell'applicazione.

## 4. Architettura e Regole d'Oro
1. **Zero I/O Core:** Il motore di manipolazione dei path NON deve toccare il File System. Deve ricevere un modello del FS, elaborare le regole di troncamento/rinomina, e restituire un "Transaction Plan".
2. **Antigravity Skills:** Utilizzare sempre i moduli di scaffolding prelevati da `sickn33/antigravity-awesome-skills`.
3. **Determinismo e Sicurezza:** Nessuna modifica al disco può avvenire senza la generazione preventiva di un Transaction Plan approvato (Dry-Run Preview).
4. **Resilienza:** Il sistema deve ignorare i permessi negati (`Access Denied`) senza crashare, riportandoli nel log.
