# 🔧 AssettoApp ACE Injector - DLL Iniettabile per Update 0.5

## ⚠️ IMPORTANTE: Approccio DLL Injection

Questo documento spiega il nuovo approccio basato su **DLL injection** per Assetto Corsa Evo Update 0.5.

### 🎯 Perché DLL Injection?

**Problema con Shared Memory in ACE Update 0.5:**
- ✅ AC originale: Shared memory funziona perfettamente
- ❌ ACE Update 0.5: Shared memory **non più accessibile/disponibile**
- 🔄 Soluzione: Iniettare DLL nel processo del gioco

### 📋 Cosa Include Questo Approccio

#### 1. **AssettoApp.ACEInjector.dll**
DLL che viene iniettata nel processo di ACE per leggere dati direttamente dalla memoria interna.

**Funzionalità:**
- Pattern scanning per trovare strutture dati in memoria
- Lettura diretta da indirizzi di memoria
- Comunicazione IPC con l'app principale
- Update continuo telemetria (100Hz)

#### 2. **Pattern Memory Scanner**
Sistema di ricerca pattern in memoria per localizzare dati:
- Supporta wildcard per offset dinamici
- Gestisce versioni diverse del gioco
- Fallback sicuro se pattern non trovati

#### 3. **Inter-Process Communication (IPC)**
Canale di comunicazione tra DLL iniettata e app principale:
- Named Pipes per trasferimento dati
- Struttura dati serializzata
- Thread-safe e performante

## 🚀 Come Funziona

### Flusso di Esecuzione

```
1. Utente avvia ACE
2. AssettoApp rileva processo ACE
3. AssettoApp inietta ACEInjector.dll nel processo
4. DLL si inizializza:
   - Trova base address modulo principale
   - Esegue pattern scanning per localizzare dati
   - Avvia thread telemetria
   - Apre canale IPC
5. Thread telemetria legge dati ogni 10ms
6. Dati inviati via IPC all'app principale
7. App principale processa e visualizza dati
```

### Struttura Pattern Scanning

```csharp
// Pattern di esempio per trovare dati velocità
byte[] pattern = { 0x48, 0x8B, 0x0D, 0x??, 0x??, 0x??, 0x?? };
string mask =     "xxx????";  // x = match esatto, ? = wildcard

IntPtr speedAddress = scanner.ScanPattern(pattern, mask);
float speed = scanner.Read<float>(speedAddress + offset);
```

## ⚙️ Configurazione e Setup

### Prerequisiti

1. **Windows 10/11 (64-bit)**
2. **.NET 8.0 SDK**
3. **Permessi Amministratore** (richiesti per injection)
4. **Assetto Corsa Evo Update 0.5** installato

### Build della DLL

```bash
cd AssettoApp.ACEInjector
dotnet build -c Release -p:Platform=x64
```

Output: `AssettoApp.ACEInjector/bin/Release/net8.0/ACEInjector.dll`

### Utilizzo

L'app principale gestirà automaticamente l'injection:

1. Rileva processo ACE
2. Inietta DLL automaticamente
3. Stabilisce connessione IPC
4. Inizia lettura telemetria

## 🔐 Considerazioni di Sicurezza

### Anti-Cheat e Rilevamento

⚠️ **ATTENZIONE**: DLL injection può essere rilevata da sistemi anti-cheat.

**Stato ACE Update 0.5:**
- ✅ Nessun anti-cheat attivo (early access)
- ⚠️ Potrebbe essere aggiunto in futuro
- 🔄 Uso solo per analisi telemetria (non modifica gameplay)

**Raccomandazioni:**
- ✅ Usa solo in modalità single-player
- ✅ Non usare in sessioni online/competitive
- ✅ Monitora changelog ACE per anti-cheat

### Permessi Richiesti

L'injection richiede permessi elevati:
- **SeDebugPrivilege** per aprire processo target
- **Permessi scrittura** nella memoria del processo
- **Amministratore** per CreateRemoteThread

## 🧪 Pattern ACE Update 0.5

### ⚠️ PLACEHOLDER - Richiedono Reverse Engineering

I pattern attuali sono **esempi** e devono essere aggiornati con analisi reale di ACE Update 0.5.

**Come ottenere pattern reali:**

1. **Usa un debugger** (x64dbg, Cheat Engine)
2. **Trova strutture dati** in memoria
3. **Identifica pattern univoci** intorno ai dati
4. **Testa stabilità** tra restart del gioco
5. **Documenta offset** per ogni campo

### Esempio: Trovare Velocità

```
1. Apri ACE in x64dbg
2. Cerca valore float della velocità (es: 150.5 km/h)
3. Muovi auto e aggiorna ricerca
4. Identifica indirizzo stabile
5. Analizza codice assembly che accede a quell'indirizzo
6. Estrai pattern byte unici
```

## 🔧 Personalizzazione Pattern

Per aggiornare pattern per nuove versioni ACE:

### 1. Modifica Pattern

File: `Memory/MemoryScanner.cs`

```csharp
public static class Patterns
{
    // Aggiorna con pattern reali
    public static readonly byte[] PhysicsData = new byte[]
    {
        // I tuoi byte qui
    };
    public static readonly string PhysicsDataMask = "xxx????xxx";
}
```

### 2. Aggiorna Offset

```csharp
public static class Offsets
{
    public const int Speed = 0x??;  // Offset reale
    public const int RPM = 0x??;
    // etc.
}
```

### 3. Test e Verifica

```csharp
// Aggiungi logging per debug
LogInfo($"Pattern found at: 0x{address:X}");
LogInfo($"Speed value: {speed}");
```

## 🐛 Troubleshooting

### DLL non si inietta

**Problema**: `Failed to inject DLL`

**Soluzioni:**
1. Esegui app come Amministratore
2. Disabilita antivirus temporaneamente
3. Verifica percorso DLL corretto
4. Controlla architettura (x64 only)

### Pattern non trovati

**Problema**: `Pattern scan returned 0x0`

**Soluzioni:**
1. Pattern obsoleti per questa versione ACE
2. Aggiorna pattern con reverse engineering
3. Verifica che ACE sia in sessione attiva (non menu)
4. Controlla che modulo base sia corretto

### Dati errati/corrotti

**Problema**: Telemetria mostra valori strani

**Soluzioni:**
1. Offset errati nella struttura dati
2. Tipo di dato sbagliato (float vs int)
3. Endianness issues
4. Versione ACE non compatibile

### Crash del gioco

**Problema**: ACE crasha dopo injection

**Soluzioni:**
1. DLL sta scrivendo in memoria protetta
2. Pattern matching errato causa accesso illegale
3. Thread telemetria troppo aggressivo
4. Riduci frequenza lettura (100Hz → 50Hz)

## 📊 Performance

### Metriche Target

- **Frequenza telemetria**: 100Hz (10ms interval)
- **CPU overhead**: < 2% su single core
- **Memory overhead**: < 10MB
- **IPC latency**: < 1ms

### Ottimizzazioni

```csharp
// Caching indirizzi dopo prima scansione
private static IntPtr _cachedSpeedAddress = IntPtr.Zero;

if (_cachedSpeedAddress == IntPtr.Zero)
    _cachedSpeedAddress = scanner.ScanPattern(...);

float speed = scanner.Read<float>(_cachedSpeedAddress);
```

## 🔄 Compatibilità Versioni ACE

| Versione ACE | Pattern Status | Note |
|--------------|----------------|------|
| Update 0.5 | 🔄 In Development | Pattern da definire |
| Update 0.6 | ❓ Unknown | Richiederà aggiornamento |
| Update 0.7+ | ❓ Unknown | Monitorare changelog |

**Strategia aggiornamento:**
- Ogni major update ACE richiede test pattern
- Mantenere database pattern per versioni multiple
- Auto-detect versione e caricare pattern corretti

## 📚 Riferimenti Tecnici

### DLL Injection Techniques

- **LoadLibrary Injection**: Metodo usato da questo progetto
- **Manual Mapping**: Alternativa più stealth (più complessa)
- **Thread Hijacking**: Per evasione anti-cheat avanzata

### Letture Consigliate

- [Windows DLL Injection](https://docs.microsoft.com/en-us/windows/win32/dlls/dynamic-link-libraries)
- [Pattern Scanning Techniques](https://www.unknowncheats.me/wiki/Pattern_Scanning)
- [x64 Assembly Basics](https://www.intel.com/content/www/us/en/developer/articles/technical/intel-sdm.html)

### Tools Utili

- **x64dbg**: Debugger per analisi binaria
- **Cheat Engine**: Memory scanner
- **Process Hacker**: Monitor processi
- **IDA Pro/Ghidra**: Disassembler per reverse engineering

## ⚖️ Note Legali

**Disclaimer:**
- Questo software è per **analisi telemetria personale** only
- Non modifica il gameplay o fornisce vantaggi competitivi
- Uso a proprio rischio
- Rispetta i Terms of Service di ACE
- Non usare per cheating o competizioni online

## 🤝 Contributi

Per contribuire pattern per Update 0.5:

1. Fork del repository
2. Reverse engineer ACE Update 0.5
3. Documenta pattern trovati
4. Test su diverse configurazioni
5. Submit Pull Request con:
   - Pattern bytes esatti
   - Offset verificati
   - Screenshot/proof che funziona
   - Note su come trovati

## 📝 Changelog

### v1.0.0 - Framework Iniziale
- ✅ Struttura DLL injector
- ✅ Pattern scanner base
- ✅ IPC foundation
- ⚠️ Pattern placeholder (richiedono RE)

### v1.1.0 - Planned
- [ ] Pattern Update 0.5 reali
- [ ] Testing con ACE
- [ ] Ottimizzazioni performance
- [ ] Auto-injection da UI

---

**Per domande o supporto, apri una Issue su GitHub.**

_Documentazione v1.0 - Febbraio 2026_
