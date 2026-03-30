# Changelog

## Release 1.0.0 - PathManager Professional Suite
### Added
- **Core Engine:** Implementato `PathResolutionEngine` per la logica di troncamento (in memoria, Zero I/O).
- **Controllo di Dominio:** Centralizzato tramite `PathTransaction`, portando consistenza applicativa per gli Audit.
- **Bypass Nativo:** Creato l'Adattatore `Win32FileSystemAdapter` che chiama `MoveFileW` per gestire e piegare i limiti Legacy dei 260 chrs nativamente sul filesystem.
- **Interfaccia Reattiva:** Progettata un'Intelligent Grid con background task mapping (`IProgress`). Il blocco della UI di scanning è stato sradicato.
- **Auditing CSV:** Aggiunto un modulo automatico robusto e silente `CsvAuditReporter` incaricato degli output di verifica post-esecuzione.

### Changed
- Refactoring complessivo aderente allo standard C# Pro (Clean Architecture, SOLID, Port & Adapters).
- `MainForm` ricostruita come un guscio per incapsulare il core.
