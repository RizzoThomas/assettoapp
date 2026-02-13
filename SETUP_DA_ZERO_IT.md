# 🚀 Guida Completa: Clonare e Configurare AssettoApp da Zero

## 📋 Indice
1. [Prerequisiti](#prerequisiti)
2. [Installazione .NET SDK](#installazione-net-sdk)
3. [Clonare il Repository](#clonare-il-repository)
4. [Compilare l'Applicazione](#compilare-lapplicazione)
5. [Eseguire l'Applicazione](#eseguire-lapplicazione)
6. [Verifica dell'Installazione](#verifica-dellinstallazione)
7. [Risoluzione Problemi](#risoluzione-problemi)
8. [Prossimi Passi](#prossimi-passi)

---

## 📦 Prerequisiti

### Sistema Operativo
- **Windows 10** (64-bit) o successivo
- **Windows 11** (64-bit) raccomandato
- ⚠️ **NON** compatibile con Linux o macOS (usa WPF e shared memory Windows)

### Software Richiesto
1. **Git** - Per clonare il repository
2. **.NET 8.0 SDK** - Per compilare l'applicazione
3. **(Opzionale) Visual Studio 2022** o **Visual Studio Code** - Per modificare il codice

---

## 🔧 Installazione .NET SDK

### Passo 1: Scaricare .NET 8.0 SDK

1. Vai su: **https://dotnet.microsoft.com/download/dotnet/8.0**
2. Scarica **".NET 8.0 SDK"** (non Runtime, serve l'SDK completo)
3. Scegli la versione **x64** per Windows

### Passo 2: Installare .NET SDK

1. Esegui il file scaricato (es. `dotnet-sdk-8.0.xxx-win-x64.exe`)
2. Segui la procedura guidata di installazione
3. Accetta i termini di licenza
4. Attendi il completamento (circa 2-3 minuti)

### Passo 3: Verificare l'Installazione

1. Apri il **Prompt dei Comandi** (cmd):
   - Premi `Windows + R`
   - Digita `cmd` e premi Invio

2. Digita il comando:
```bash
dotnet --version
```

3. Dovresti vedere l'output tipo:
```
8.0.100
```

✅ Se vedi un numero di versione 8.x.x, .NET SDK è installato correttamente!

❌ Se vedi "comando non riconosciuto":
- Riavvia il computer dopo l'installazione
- Controlla che l'installazione sia completata correttamente
- Prova a reinstallare .NET SDK

---

## 📥 Clonare il Repository

### Opzione A: Con Git dalla Command Line (Raccomandato)

#### Passo 1: Installare Git (se non già installato)
1. Scarica Git da: **https://git-scm.com/download/win**
2. Installa con le opzioni predefinite
3. Riavvia il Prompt dei Comandi

#### Passo 2: Clonare il Repository
```bash
# Naviga alla cartella dove vuoi clonare il progetto
# Ad esempio, in Documenti:
cd %USERPROFILE%\Documents

# Clona il repository
git clone https://github.com/RizzoThomas/assettoapp.git

# Entra nella cartella del progetto
cd assettoapp
```

### Opzione B: Download ZIP (Alternativa)

Se non hai Git installato:

1. Vai su: **https://github.com/RizzoThomas/assettoapp**
2. Clicca sul pulsante verde **"Code"**
3. Seleziona **"Download ZIP"**
4. Estrai il file ZIP in una cartella a tua scelta
5. Apri il Prompt dei Comandi e naviga alla cartella estratta:
```bash
cd C:\percorso\dove\hai\estratto\assettoapp
```

---

## 🔨 Compilare l'Applicazione

### Metodo 1: Automatico con Script Batch (Più Facile) ⭐

#### Passo 1: Eseguire lo Script di Build
```bash
# Assicurati di essere nella cartella del progetto
cd %USERPROFILE%\Documents\assettoapp

# Esegui lo script di build
build.bat
```

Lo script farà automaticamente:
- Pulizia dei build precedenti
- Ripristino delle dipendenze NuGet
- Compilazione in modalità Release
- Creazione dell'eseguibile autonomo

**Tempo stimato:** 2-3 minuti

#### Passo 2: Trovare l'Eseguibile

Dopo la compilazione, troverai l'applicazione pronta qui:
```
AssettoApp.UI\bin\Release\net8.0-windows\win-x64\publish\AssettoApp.UI.exe
```

💾 **Dimensione:** ~150 MB (include tutto il necessario, .NET runtime incluso)

---

### Metodo 2: Manuale con Comandi (Per Esperti)

Se preferisci avere più controllo:

#### Passo 1: Ripristinare le Dipendenze
```bash
dotnet restore
```

Questo scarica tutti i pacchetti NuGet necessari (LiveCharts, Microsoft.Extensions.DependencyInjection).

#### Passo 2: Compilare in Debug (per test rapidi)
```bash
dotnet build AssettoApp.UI/AssettoApp.UI.csproj -c Debug
```

Output in: `AssettoApp.UI\bin\Debug\net8.0-windows\`

#### Passo 3: Compilare in Release (per uso finale)
```bash
dotnet build AssettoApp.UI/AssettoApp.UI.csproj -c Release
```

Output in: `AssettoApp.UI\bin\Release\net8.0-windows\`

#### Passo 4: Creare Eseguibile Autonomo (Self-Contained)
```bash
dotnet publish AssettoApp.UI/AssettoApp.UI.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:EnableCompressionInSingleFile=true ^
    -p:PublishReadyToRun=true
```

Questo crea un singolo file .exe che include .NET runtime.

---

### Metodo 3: Con Visual Studio 2022

Se hai Visual Studio installato:

1. Apri **Visual Studio 2022**
2. File → Apri → Progetto/Soluzione
3. Seleziona `AssettoApp.slnx`
4. In alto, cambia la configurazione da "Debug" a **"Release"**
5. Menu Build → **Build Solution** (o premi `Ctrl+Shift+B`)
6. Tasto destro sul progetto `AssettoApp.UI` → **Publish**
7. Segui la procedura guidata per pubblicare

---

## ▶️ Eseguire l'Applicazione

### Prima Esecuzione

#### Opzione A: Eseguibile Autonomo
```bash
# Naviga alla cartella publish
cd AssettoApp.UI\bin\Release\net8.0-windows\win-x64\publish

# Esegui l'applicazione
AssettoApp.UI.exe
```

Oppure semplicemente fai **doppio click** su `AssettoApp.UI.exe` da Esplora File.

#### Opzione B: Tramite dotnet run (Debug)
```bash
# Dalla cartella principale del progetto
dotnet run --project AssettoApp.UI/AssettoApp.UI.csproj
```

### Al Primo Avvio

L'applicazione si aprirà con l'interfaccia principale:
- ✅ Se vedi la finestra: tutto funziona!
- ❌ Se ottieni errori, vedi la sezione [Risoluzione Problemi](#risoluzione-problemi)

---

## ✅ Verifica dell'Installazione

### Checklist di Verifica

Completa questi passaggi per verificare che tutto funzioni:

- [ ] **.NET SDK installato**: `dotnet --version` mostra versione 8.x.x
- [ ] **Repository clonato**: Cartella `assettoapp` esiste
- [ ] **Build completata**: Nessun errore durante `build.bat` o `dotnet build`
- [ ] **Eseguibile creato**: File `AssettoApp.UI.exe` esiste
- [ ] **App si avvia**: Finestra dell'applicazione appare
- [ ] **Auto-rilevamento funziona**: Click su "🔍 Auto-Detect Running Game"

### Test Rapido (Senza Simulatore)

1. Avvia l'applicazione
2. Seleziona modalità **"Game Closed"** (Online Analysis mode)
3. Seleziona una combinazione auto/tracciato
4. Click su **"Generate Preset Setup"**
5. ✅ Se vedi i parametri del setup, tutto funziona!

### Test Completo (Con Assetto Corsa)

1. Avvia **Assetto Corsa**
2. Entra in una sessione di pratica
3. Avvia **AssettoApp**
4. Click su **"🔍 Auto-Detect Running Game"**
5. Click su **"Connect to Simulator"**
6. ✅ Se dice "Connected", l'integrazione funziona!

---

## 🔧 Risoluzione Problemi

### Problema: "dotnet non riconosciuto come comando"

**Soluzione:**
1. Reinstalla .NET 8.0 SDK
2. Riavvia il computer
3. Apri un nuovo Prompt dei Comandi
4. Verifica con `dotnet --version`

### Problema: "Build fallita - NU1101"

**Errore:** Pacchetto non trovato

**Soluzione:**
```bash
# Pulisci la cache NuGet
dotnet nuget locals all --clear

# Ripristina di nuovo
dotnet restore --force

# Riprova il build
dotnet build -c Release
```

### Problema: "Could not find a part of the path"

**Soluzione:**
- Il percorso è troppo lungo per Windows
- Sposta il progetto in una cartella con percorso più corto
- Es. da `C:\Users\Nome\Documents\Projects\...` a `C:\Projects\assettoapp`

### Problema: "Platform mismatch - x86 vs x64"

**Soluzione:**
```bash
# Specifica esplicitamente la piattaforma
dotnet build -c Release /p:Platform=x64
```

### Problema: Build.bat non si avvia

**Soluzione:**
1. Tasto destro su `build.bat`
2. Seleziona "Modifica" (si apre con Notepad)
3. Controlla che le righe non abbiano caratteri strani
4. Salva e riprova
5. Oppure esegui i comandi manualmente (Metodo 2)

### Problema: "Failed to connect to simulator"

**Soluzione:**
- Assicurati che AC/ACE sia **in esecuzione**
- Devi essere **in pista**, non nel menu
- Prova a eseguire AssettoApp come **amministratore**
- Per ACE: l'API telemetria potrebbe non essere ancora disponibile

### Problema: Antivirus blocca l'eseguibile

**Soluzione:**
- Aggiungi un'eccezione nell'antivirus per la cartella del progetto
- Oppure disabilita temporaneamente l'antivirus durante il build
- Il codice è open source, puoi verificarlo personalmente

### Problema: "WPF not supported on this platform"

**Soluzione:**
- Assicurati di essere su **Windows** (non Linux/macOS)
- WPF richiede Windows 10 o superiore
- Verifica che stai compilando con `-r win-x64`

---

## 🎯 Prossimi Passi

### 1. Configurare per il Tuo Simulatore

#### Per Assetto Corsa (AC)
1. Installa Assetto Corsa da Steam
2. Avvia AC almeno una volta
3. AssettoApp rileverà automaticamente il processo `ac.exe`

#### Per Assetto Corsa EVO (ACE)
1. Installa ACE da Steam
2. AssettoApp cercherà `AC2.exe`, `AssettoCorsa2.exe`, o `assettocorsaevo.exe`
3. ⚠️ **Nota:** ACE potrebbe non esporre ancora l'API telemetria

### 2. Leggere la Documentazione

Familiarizza con le guide:
- **README.md** - Panoramica generale e features
- **ACE_INTEGRATION_IT.md** - Guida specifica per ACE
- **ARCHITECTURE.md** - Dettagli tecnici architettura
- **BUILD_IT.md** - Opzioni di build avanzate

### 3. Primo Utilizzo

Segui il workflow completo:
1. Avvia il simulatore (AC o ACE)
2. Entra in una sessione
3. Connetti AssettoApp
4. Registra 5-10 giri
5. Analizza i dati
6. Genera setup ottimizzato
7. Esporta in formato .INI
8. Carica il setup nel gioco

### 4. Personalizzare l'Applicazione

Se sei uno sviluppatore:
- Esplora il codice sorgente in Visual Studio
- Modifica i parametri di analisi in `TelemetryAnalyzer.cs`
- Adatta la generazione setup in `SetupGenerator.cs`
- Crea nuovi connettori per altri simulatori

### 5. Contribuire al Progetto

Vuoi migliorare l'app?
1. Crea un fork su GitHub
2. Fai le tue modifiche
3. Testa accuratamente
4. Crea una Pull Request
5. Descrivi le modifiche in dettaglio

---

## 📊 Struttura del Progetto

Per orientarti nel codice:

```
assettoapp/
├── AssettoApp.Core/              # Modelli e interfacce base
│   ├── Models/                   # TelemetryData, CarSetup, GameType
│   └── Interfaces/               # ISimulatorConnector, ITelemetryAnalyzer
│
├── AssettoApp.SimulatorIntegration/  # Connessione ai simulatori
│   ├── Connectors/               # AssettoCorساConnector, ACEConnector
│   └── SharedMemory/             # Lettura memoria condivisa
│
├── AssettoApp.TelemetryAnalysis/     # Analisi dati telemetrici
│   └── TelemetryAnalyzer.cs      # 6 algoritmi di analisi
│
├── AssettoApp.SetupGeneration/       # Generazione setup
│   └── SetupGenerator.cs         # Ottimizzazione 8 categorie
│
├── AssettoApp.OnlineData/            # Dati online e preset
│   └── Repositories/             # Preset cars/tracks
│
└── AssettoApp.UI/                    # Interfaccia utente WPF
    ├── MainWindow.xaml           # Layout UI
    ├── ViewModels/               # Logica applicazione
    └── Commands/                 # Pattern Command MVVM
```

---

## 🆘 Supporto e Aiuto

### Risorse Disponibili

- 📖 **Documentazione**: Leggi i file `.md` nella root del progetto
- 🐛 **Bug Report**: Apri una Issue su GitHub
- 💡 **Feature Request**: Apri una Discussion su GitHub
- ❓ **Domande**: Controlla le Issue esistenti prima di chiedere

### Informazioni da Includere nei Report

Quando segnali un problema, includi:
- Sistema operativo e versione (es. Windows 11 Pro 22H2)
- Versione .NET SDK (`dotnet --version`)
- Messaggio di errore completo
- Passi per riprodurre il problema
- Screenshot se possibile

---

## 📝 Riepilogo Comandi Rapidi

### Setup Completo in 5 Comandi

```bash
# 1. Clona il repository
git clone https://github.com/RizzoThomas/assettoapp.git
cd assettoapp

# 2. Verifica .NET SDK
dotnet --version

# 3. Ripristina dipendenze
dotnet restore

# 4. Compila
dotnet build -c Release

# 5. Esegui
dotnet run --project AssettoApp.UI/AssettoApp.UI.csproj
```

### Oppure Usa lo Script Automatico

```bash
# Su Windows
build.bat

# Su Linux/Mac (solo per test, non funzionale)
./build.sh
```

---

## ✨ Conclusione

Ora hai tutto il necessario per:
- ✅ Clonare il repository da zero
- ✅ Installare le dipendenze richieste
- ✅ Compilare l'applicazione
- ✅ Eseguire e testare AssettoApp
- ✅ Risolvere problemi comuni

### Cosa Fare Ora?

1. **Compila l'app** seguendo questa guida
2. **Provala** con Assetto Corsa
3. **Esplora le features** (telemetria, analisi, setup)
4. **Leggi la documentazione** per approfondire
5. **Contribuisci** con feedback o miglioramenti!

---

**Buon divertimento con AssettoApp! 🏁🏎️💨**

Per domande o problemi, apri una Issue su GitHub.

---

_Ultima revisione: Febbraio 2026_
