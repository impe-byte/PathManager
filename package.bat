@echo off
setlocal
echo ==============================================
echo   Path Manager Professional - Package Script
echo ==============================================
echo.

echo [1/4] Starting Build...
call build.bat
if %errorlevel% neq 0 (
    echo [ERROR] Build failed, aborting package.
    exit /b 1
)

echo.
echo [2/4] Preparing Release Directory...
set RELEASE_DIR=PathManager_Release
if exist "%RELEASE_DIR%" rmdir /s /q "%RELEASE_DIR%"
mkdir "%RELEASE_DIR%"

echo [3/4] Copying artifacts...
copy PathManagerProfessional.exe "%RELEASE_DIR%\" >nul
copy README.md "%RELEASE_DIR%\" >nul
copy CHANGELOG.md "%RELEASE_DIR%\" >nul

echo [4/4] Zipping package...
if exist "PathManager_v1.0.0.zip" del "PathManager_v1.0.0.zip"
powershell -Command "Compress-Archive -Path '%RELEASE_DIR%\*' -DestinationPath 'PathManager_v1.0.0.zip'"

echo Cleaning up...
rmdir /s /q "%RELEASE_DIR%"

echo.
echo [SUCCESS] Package PathManager_v1.0.0.zip has been created!
endlocal
