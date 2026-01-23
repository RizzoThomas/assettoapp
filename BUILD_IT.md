# Come Compilare AssettoApp - Guida Rapida

## 🚀 Metodo Rapido (Windows)

### 1. Prerequisiti
- **Windows 10/11** (64-bit)
- **.NET 8.0 SDK** - [Scarica qui](https://dotnet.microsoft.com/download/dotnet/8.0)

### 2. Verifica Installazione .NET
Apri il Prompt dei Comandi (cmd) e digita:
```bash
dotnet --version
```
Dovresti vedere qualcosa come `8.0.xxx`.

### 3. Build Automatico con Script

**Opzione A - Script Batch (Raccomandato):**
1. Doppio click su `build.bat`
2. Aspetta che finisca (circa 1-2 minuti)
3. Trova l'eseguibile in: `AssettoApp.UI\bin\Release\net8.0-windows\win-x64\publish\AssettoApp.UI.exe`

**Opzione B - Manualmente:**

```bash
# 1. Apri il Prompt dei Comandi nella cartella del progetto
cd C:\path\to\assettoapp

# 2. Ripristina dipendenze
dotnet restore

# 3. Compila in modalità Release
dotnet build AssettoApp.UI/AssettoApp.UI.csproj -c Release

# 4. Crea eseguibile autonomo (include .NET runtime)
dotnet publish AssettoApp.UI/AssettoApp.UI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 📁 Dove Trovare l'Eseguibile

Dopo la compilazione, l'eseguibile sarà qui:
```
AssettoApp.UI\bin\Release\net8.0-windows\win-x64\publish\AssettoApp.UI.exe
```

**Dimensione file:** ~150 MB (include tutto il necessario, non serve installare .NET)

## ▶️ Come Usare

1. **Avvia Assetto Corsa** e inizia una sessione (pratica o gara)
2. **Avvia AssettoApp.exe**
3. Seleziona "Assetto Corsa" dal menu a tendina
4. Clicca "Connetti al Simulatore"
5. Seleziona auto e circuito
6. Clicca "Avvia Registrazione"
7. Guida per alcuni giri (almeno 5-10)
8. Clicca "Ferma Registrazione"
9. Clicca "Analizza Sessione" per vedere i risultati
10. Clicca "Genera Setup" per creare il setup ottimizzato
11. Clicca "Esporta Setup" per salvare il file .INI

## 📂 Installare il Setup in Assetto Corsa

Dopo aver esportato il setup:

1. Copia il file `.ini` esportato
2. Incollalo in: `Documenti\Assetto Corsa\setups\[nome_auto]\`
3. Avvia AC e carica il setup dal menu setup dell'auto

## ⚠️ Risoluzione Problemi

### "dotnet non è riconosciuto come comando..."
- Installa .NET 8.0 SDK da Microsoft
- Riavvia il Prompt dei Comandi dopo l'installazione

### "Connessione fallita al simulatore"
- Assicurati che Assetto Corsa sia in esecuzione
- Devi essere in una sessione attiva (non nel menu principale)
- Prova a eseguire AssettoApp come amministratore

### Build fallisce con errori
- Esegui: `dotnet clean` e poi riprova
- Controlla di avere Windows 10/11 (64-bit)
- Verifica che .NET 8.0 SDK sia installato correttamente

### L'applicazione si blocca o non risponde
- Chiudi e riavvia l'applicazione
- Assicurati che AC sia ancora in esecuzione
- Controlla che non ci siano antivirus che bloccano l'app

## 🔧 Build per Sviluppatori

### Debug Build (più veloce, per testing)
```bash
dotnet build AssettoApp.UI/AssettoApp.UI.csproj -c Debug
```
Eseguibile in: `AssettoApp.UI\bin\Debug\net8.0-windows\`

### Framework-Dependent (più piccolo, richiede .NET installato)
```bash
dotnet publish AssettoApp.UI/AssettoApp.UI.csproj -c Release -r win-x64 --self-contained false
```
Dimensione: ~5 MB (ma richiede .NET 8.0 installato sul PC target)

### Con Visual Studio 2022
1. Apri `AssettoApp.slnx`
2. Seleziona "Release" dalla barra strumenti
3. Build → Build Solution (Ctrl+Shift+B)
4. Tasto destro su AssettoApp.UI → Publish
5. Segui la procedura guidata

## 📊 Informazioni Tecniche

- **Linguaggio:** C# 12
- **Framework:** .NET 8.0
- **UI:** WPF (Windows Presentation Foundation)
- **Architettura:** MVVM con Dependency Injection
- **Piattaforma:** Windows 10/11 x64

## 🎮 Supporto Simulatori

- ✅ **Assetto Corsa (AC):** Completamente funzionante via shared memory
- ⚠️ **Assetto Corsa EVO (ACE):** In attesa di API telemetria (early access)

## 📚 Documentazione Completa

- `README.md` - Panoramica generale
- `BUILD.md` - Istruzioni dettagliate di build (inglese)
- `ARCHITECTURE.md` - Dettagli tecnici architettura
- `IMPLEMENTAZIONE_IT.md` - Riepilogo implementazione (italiano)

## 🆘 Supporto

Per problemi, bug o richieste:
1. Controlla la documentazione completa
2. Apri un issue su GitHub
3. Includi: versione Windows, versione .NET, messaggio errore completo

---

**Buona guida! 🏁**
