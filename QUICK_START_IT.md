# 🎯 AssettoApp - Quick Start Card

## ⚡ Setup Rapido (5 Minuti)

### 1️⃣ Prerequisiti
- Windows 10/11 (64-bit)
- .NET 8.0 SDK: https://dotnet.microsoft.com/download/dotnet/8.0

### 2️⃣ Clone
```bash
git clone https://github.com/RizzoThomas/assettoapp.git
cd assettoapp
```

### 3️⃣ Build
```bash
build.bat
```

### 4️⃣ Esegui
```bash
AssettoApp.UI\bin\Release\net8.0-windows\win-x64\publish\AssettoApp.UI.exe
```

---

## 📝 Comandi Essenziali

| Azione | Comando |
|--------|---------|
| Verifica .NET | `dotnet --version` |
| Clone repo | `git clone https://github.com/RizzoThomas/assettoapp.git` |
| Ripristina deps | `dotnet restore` |
| Build Debug | `dotnet build -c Debug` |
| Build Release | `dotnet build -c Release` |
| Publish EXE | `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true` |
| Run diretto | `dotnet run --project AssettoApp.UI/AssettoApp.UI.csproj` |
| Pulisci build | `dotnet clean` |

---

## 🎮 Workflow Utilizzo

1. **Avvia AC/ACE** → Entra in sessione
2. **Avvia AssettoApp** → Auto-Detect
3. **Connetti** → "Connect to Simulator"
4. **Registra** → "Start Recording" (5-10 giri)
5. **Ferma** → "Stop Recording"
6. **Analizza** → "Analyze Session"
7. **Genera** → "Generate Setup"
8. **Esporta** → "Export Setup"

---

## 🔧 Problemi Comuni

| Problema | Soluzione |
|----------|-----------|
| "dotnet not found" | Installa .NET SDK + riavvia |
| Build fallisce | `dotnet clean` poi rebuild |
| Path troppo lungo | Sposta in `C:\Projects\assettoapp` |
| Connection failed | AC deve essere in pista, non nel menu |
| Antivirus blocca | Aggiungi eccezione per la cartella |

---

## 📚 Documentazione

- **Setup completo**: [SETUP_DA_ZERO_IT.md](SETUP_DA_ZERO_IT.md)
- **Build avanzato**: [BUILD_IT.md](BUILD_IT.md)
- **ACE integration**: [ACE_INTEGRATION_IT.md](ACE_INTEGRATION_IT.md)
- **Architecture**: [ARCHITECTURE.md](ARCHITECTURE.md)
- **Features**: [README.md](README.md)

---

## 📂 Percorsi Output

| Build Type | Percorso |
|------------|----------|
| Debug | `AssettoApp.UI\bin\Debug\net8.0-windows\` |
| Release | `AssettoApp.UI\bin\Release\net8.0-windows\` |
| Published | `AssettoApp.UI\bin\Release\net8.0-windows\win-x64\publish\` |

---

## 💡 Tips

- 🎯 Usa `build.bat` per build automatico completo
- 🚀 Build Release è 3x più veloce di Debug
- 💾 Published EXE è ~150MB (include .NET)
- 🔄 Usa `dotnet run` per test rapidi
- 📊 Registra almeno 5 giri per analisi accurate
- 🏁 Perfect Laps = no track limits violations

---

## 🆘 Supporto

**GitHub Issues**: https://github.com/RizzoThomas/assettoapp/issues

Includi sempre:
- OS e versione
- Output di `dotnet --version`
- Messaggio di errore completo

---

_Quick Reference v1.0 - Febbraio 2026_
