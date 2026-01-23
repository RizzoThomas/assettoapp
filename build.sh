#!/bin/bash
# Script di build per AssettoApp su Linux/Mac (solo per sviluppo/test)
# NOTA: L'applicazione richiede Windows per funzionare (WPF + shared memory)

set -e

echo "========================================"
echo "AssettoApp - Script di Build"
echo "========================================"
echo ""

# Controlla se .NET SDK è installato
if ! command -v dotnet &> /dev/null; then
    echo "ERRORE: .NET SDK non trovato!"
    echo "Scarica e installa .NET 8.0 SDK da:"
    echo "https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

echo "Versione .NET SDK:"
dotnet --version
echo ""

echo "[1/3] Pulizia build precedenti..."
dotnet clean AssettoApp.UI/AssettoApp.UI.csproj -c Release > /dev/null 2>&1 || true

echo "[2/3] Ripristino dipendenze..."
dotnet restore AssettoApp.UI/AssettoApp.UI.csproj

echo "[3/3] Compilazione progetto (Release)..."
dotnet build AssettoApp.UI/AssettoApp.UI.csproj -c Release

if [ $? -eq 0 ]; then
    echo ""
    echo "========================================"
    echo "BUILD COMPLETATA CON SUCCESSO!"
    echo "========================================"
    echo ""
    echo "NOTA: Per creare l'eseguibile .exe Windows, esegui su Windows:"
    echo "  dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true"
    echo ""
else
    echo ""
    echo "ERRORE: Compilazione fallita!"
    exit 1
fi
