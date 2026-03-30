@echo off
setlocal
echo ==============================================
echo   Path Manager Professional - Build Script
echo ==============================================
echo.

set COMPILER="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist %COMPILER% (
    set COMPILER="C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)

if not exist %COMPILER% (
    echo [ERROR] C# Compiler not found on this system!
    exit /b 1
)

echo Compiling source files...
%COMPILER% /nologo /target:winexe /out:PathManagerProfessional.exe src\Program.cs src\Core\Domain\*.cs src\Core\Engine\*.cs src\Core\Application\*.cs src\Core\Ports\*.cs src\Infrastructure\*.cs src\UI\MainForm.cs

if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Compilation failed!
) else (
    echo.
    echo [SUCCESS] Compilation completed: PathManagerProfessional.exe generated!
)
endlocal
