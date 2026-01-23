@echo off
REM Script di build per AssettoApp
REM Richiede .NET 8.0 SDK installato

echo ========================================
echo AssettoApp - Script di Build
echo ========================================
echo.

REM Controlla se .NET SDK è installato
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERRORE: .NET SDK non trovato!
    echo Scarica e installa .NET 8.0 SDK da:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo [1/4] Pulizia build precedenti...
dotnet clean AssettoApp.UI/AssettoApp.UI.csproj -c Release >nul 2>&1

echo [2/4] Ripristino dipendenze...
dotnet restore AssettoApp.UI/AssettoApp.UI.csproj

if %errorlevel% neq 0 (
    echo ERRORE: Ripristino dipendenze fallito!
    pause
    exit /b 1
)

echo [3/4] Compilazione progetto (Release)...
dotnet build AssettoApp.UI/AssettoApp.UI.csproj -c Release

if %errorlevel% neq 0 (
    echo ERRORE: Compilazione fallita!
    pause
    exit /b 1
)

echo [4/4] Creazione eseguibile autonomo...
dotnet publish AssettoApp.UI/AssettoApp.UI.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:EnableCompressionInSingleFile=true ^
    -p:PublishReadyToRun=true

if %errorlevel% neq 0 (
    echo ERRORE: Pubblicazione fallita!
    pause
    exit /b 1
)

echo.
echo ========================================
echo BUILD COMPLETATA CON SUCCESSO!
echo ========================================
echo.
echo Eseguibile creato in:
echo AssettoApp.UI\bin\Release\net8.0-windows\win-x64\publish\AssettoApp.UI.exe
echo.
echo Dimensione approssimativa: ~150 MB (include .NET runtime)
echo.
pause
