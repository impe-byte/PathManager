# Path Manager Professional

**Path Manager Professional** is a high-performance Windows desktop application designed to traverse deep directory structures and identify files or folders whose paths exceed a specified length threshold. It is extremely useful for migrating data to SharePoint, OneDrive, or new NAS systems where Windows' historical `MAX_PATH` limitation (260 characters) often causes unpredictable synchronization failures.

## 🚀 Features
- **Unstoppable Core Engine**: Interacts directly with Win32 `kernel32.dll` APIs (`WIN32_FIND_DATAW`), effortlessly bypassing the standard .NET 260-character limitation and natively skipping `Access Denied` folders without crashing.
- **Multilingual Support**: Fully localized in English and Italian. Switch instantly via the UI toggle button.
- **Advanced Export System**: Generates reports in three formats:
  - `TXT`: Standard log summary.
  - `CSV`: Spreadsheets compatible layout.
  - `HTML`: A beautiful, responsive, and interactive HTML report with built-in Javascript sorting and search filtering capabilities.
- **Modern UI**: A sleek, user-friendly graphical interface with async progress reporting that never freezes the main thread.
- **Standalone**: Zero dependencies required.

## 🛠️ How to Build
This project requires **NO** third-party dependencies, package managers, or installations (Visual Studio is completely optional).

Simply open the root folder and double-click the `build.bat` script on any Windows machine to instantly compile the modular source code into a self-contained executable: `PathManagerProfessional.exe`.

## 📂 Project Structure
- `src/Core/`: Native scanning engine and data models.
- `src/UI/`: WinForms interfaces and design logic.
- `src/Exporters/`: Modular system for exporting `ScanReport` objects.
- `src/Localization/`: Static lightweight dictionary engine for `i18n` support.

## 📜 License
MIT License.
