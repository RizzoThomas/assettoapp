# 🔍 Come Completare l'Implementazione - Guida Reverse Engineering

## 🎯 Obiettivo

Completare `AssettoApp.ACEInjector` con pattern e offset reali per Assetto Corsa Evo Update 0.5.

---

## 📋 Checklist Implementazione

### ✅ Fase 1: Preparazione (COMPLETATA)
- [x] Struttura progetto DLL
- [x] Memory scanner framework
- [x] Entry points DLL
- [x] Documentazione base

### ⏳ Fase 2: Reverse Engineering (RICHIESTO)
- [ ] Installare tools (x64dbg, Cheat Engine)
- [ ] Analizzare memoria ACE Update 0.5
- [ ] Trovare indirizzi dati telemetria
- [ ] Estrarre pattern byte
- [ ] Documentare offset

### ⏳ Fase 3: Implementation (DOPO RE)
- [ ] Aggiornare pattern in `MemoryScanner.cs`
- [ ] Implementare IPC (Named Pipes)
- [ ] Testare lettura dati
- [ ] Integrare con UI

### ⏳ Fase 4: Testing & Polish
- [ ] Test con ACE reale
- [ ] Ottimizzazioni performance
- [ ] Error handling robusto
- [ ] Documentazione finale

---

## 🛠️ Tools Necessari

### 1. x64dbg (Debugger)
**Download**: https://x64dbg.com/

**Uso:**
```
1. Avvia ACE Update 0.5
2. Attach x64dbg al processo AC2.exe
3. Pausa esecuzione (F12)
4. Memory map → trova modulo principale
5. Memory search → cerca valori float (velocità, RPM)
```

### 2. Cheat Engine
**Download**: https://www.cheatengine.org/

**Uso:**
```
1. Attach a processo ACE
2. First Scan: Cerca valore (es: velocità 150)
3. Cambia valore nel gioco
4. Next Scan: Nuovo valore
5. Ripeti fino a trovare indirizzo stabile
6. Analizza codice assembly che accede all'indirizzo
```

### 3. Process Hacker
**Download**: https://processhacker.sourceforge.io/

**Uso:**
- Monitorare memoria processo
- Verificare DLL caricati
- Analizzare thread

---

## 🔬 Procedura Reverse Engineering Dettagliata

### Step 1: Trovare Velocità

#### Con Cheat Engine:

1. **First Scan**
   ```
   - Avvia ACE e entra in pista
   - Scan Type: "Exact Value"
   - Value Type: "Float"
   - Value: [velocità attuale in km/h, es: 0]
   - Click "First Scan"
   ```

2. **Next Scans**
   ```
   - Accelera l'auto
   - Leggi velocità (es: 50 km/h)
   - Inserisci nuovo valore
   - Click "Next Scan"
   - Ripeti 3-4 volte
   ```

3. **Trovato Indirizzo**
   ```
   - Dovresti avere 1-5 indirizzi
   - Aggiungi alla lista (doppio click)
   - Verifica che cambia in real-time
   - Nota indirizzo (es: 0x7FF123456789)
   ```

4. **Estrai Pattern**
   ```
   - Click destro sull'indirizzo
   - "Find out what accesses this address"
   - Guida l'auto
   - Vedi codice assembly che legge/scrive
   - Esempio:
     mov eax, [rcx+0x40]  // Legge velocità
     
   - Indirizzo instruction: 0x7FF000A12345
   - Pattern intorno: 48 8B 41 40 F3 0F 10 00
   ```

5. **Crea Pattern in C#**
   ```csharp
   public static readonly byte[] SpeedPattern = new byte[]
   {
       0x48, 0x8B, 0x41, 0x40,  // mov eax, [rcx+0x40]
       0xF3, 0x0F, 0x10, 0x00   // movss xmm0, [eax]
   };
   public static readonly string SpeedMask = "xxxxxxxx";
   
   // Se alcuni byte cambiano tra restart:
   public static readonly byte[] SpeedPattern = new byte[]
   {
       0x48, 0x8B, 0x41, 0x00,  // mov eax, [rcx+offset]
       0xF3, 0x0F, 0x10, 0x00   // movss xmm0, [eax]
   };
   public static readonly string SpeedMask = "xxx?xxx?";
   ```

### Step 2: Trovare Altri Dati

Ripeti processo per:
- RPM (int o float)
- Gear (int)
- Throttle (float 0-1)
- Brake (float 0-1)
- Position X,Y,Z (float[3])
- Tire Temperature (float[4])
- Lap Time (int milliseconds)

### Step 3: Determinare Struttura Dati

Una volta trovati gli indirizzi, calcola offset relativi:

```
Esempio:
Speed address:    0x1A2B3C40
RPM address:      0x1A2B3C44
Gear address:     0x1A2B3C48
Position X:       0x1A2B3C60

Struttura:
struct VehicleData
{
    float Speed;      // offset 0x00
    float RPM;        // offset 0x04
    int Gear;         // offset 0x08
    // padding 0x0C-0x5F
    float PositionX;  // offset 0x60
    ...
}
```

### Step 4: Implementare in Codice

**File**: `AssettoApp.ACEInjector/Memory/ACEPatterns.cs`

```csharp
public static class ACEPatterns
{
    // Pattern trovato con RE
    public static readonly byte[] VehicleDataPattern = new byte[]
    {
        0x48, 0x8B, 0x0D, 0x??, 0x??, 0x??, 0x??,  // mov rcx, [rip+offset]
        0x48, 0x85, 0xC9,                          // test rcx, rcx
        0x74, 0x??                                 // jz short
    };
    public static readonly string VehicleDataMask = "xxx????xxx?x?";
    
    public static class Offsets
    {
        // Offset reali trovati con RE
        public const int Speed = 0x00;
        public const int RPM = 0x04;
        public const int Gear = 0x08;
        public const int Throttle = 0x0C;
        public const int PositionX = 0x60;
        public const int PositionY = 0x64;
        public const int PositionZ = 0x68;
        // etc...
    }
}
```

---

## 🧪 Testing dei Pattern

### Test 1: Pattern Stability

```csharp
// Test se pattern funziona dopo restart
for (int i = 0; i < 5; i++)
{
    // Restart ACE
    // Re-scan pattern
    IntPtr addr = scanner.ScanPattern(pattern, mask);
    Console.WriteLine($"Attempt {i}: 0x{addr:X}");
}

// Se indirizzo cambia ma pattern funziona: ✅ OK
// Se pattern non trova più: ❌ Serve pattern più robusto
```

### Test 2: Data Validation

```csharp
// Verifica che dati abbiano senso
float speed = scanner.Read<float>(speedAddr);
if (speed < 0 || speed > 400)  // km/h ragionevole
{
    Console.WriteLine("ERROR: Speed value invalid!");
}

int rpm = scanner.Read<int>(rpmAddr);
if (rpm < 0 || rpm > 12000)  // RPM ragionevole
{
    Console.WriteLine("ERROR: RPM value invalid!");
}
```

### Test 3: Update Frequency

```csharp
// Verifica che valori cambino in real-time
float lastSpeed = 0;
int unchangedCount = 0;

for (int i = 0; i < 100; i++)
{
    float speed = scanner.Read<float>(speedAddr);
    if (Math.Abs(speed - lastSpeed) < 0.1f)
        unchangedCount++;
    lastSpeed = speed;
    Thread.Sleep(10);
}

Console.WriteLine($"Unchanged: {unchangedCount}/100");
// Se > 90: probabilmente indirizzo sbagliato (statico)
```

---

## 📝 Template Pattern Documentation

Quando trovi pattern, documentali così:

```markdown
## Pattern: Vehicle Speed

**ACE Version**: Update 0.5
**Found Date**: 2026-02-13
**Author**: [Your Name]

### Pattern Bytes
```
48 8B 41 40 F3 0F 10 00
```

### Mask
```
xxxxxxxx
```

### Assembly
```asm
mov eax, [rcx+0x40]  ; Get pointer to vehicle data
movss xmm0, [eax]    ; Load speed float
```

### How Found
1. Used Cheat Engine to scan for speed value
2. Found address 0x1A2B3C40
3. Used "Find what accesses" to get assembly code
4. Extracted 8 bytes before instruction
5. Verified stable across 3 game restarts

### Offset in Structure
```
+0x00: Speed (float)
```

### Validation
- Value range: 0-400 km/h
- Updates at ~60Hz
- Tested on: Monza, Spa, Brands Hatch
```

---

## 🚨 Problemi Comuni

### Pattern Non Trovato

**Cause:**
- Pattern troppo specifico (usa più wildcard)
- Offset cambiati in update
- Modulo base sbagliato

**Soluzione:**
```csharp
// Pattern più flessibile
byte[] pattern = {
    0x48, 0x8B, 0x??, 0x??,  // mov rcx, [reg+??]
    0x??                      // Ignora resto
};
string mask = "xx???";
```

### Valori Errati

**Cause:**
- Offset sbagliato
- Tipo dato sbagliato (int vs float)
- Endianness

**Soluzione:**
```csharp
// Test tutti gli offset vicini
for (int offset = 0; offset < 0x100; offset += 4)
{
    float val = scanner.Read<float>(baseAddr + offset);
    Console.WriteLine($"Offset 0x{offset:X}: {val}");
}
```

### Crash del Gioco

**Cause:**
- Accesso a memoria protetta
- Pattern matching su codice invece che dati
- Lettura fuori bounds

**Soluzione:**
```csharp
// Aggiungi bounds checking
if (address == IntPtr.Zero || address.ToInt64() < 0x1000)
{
    LogError("Invalid address");
    return;
}

// Usa try-catch
try
{
    float val = scanner.Read<float>(address);
}
catch (AccessViolationException)
{
    LogError("Access violation at " + address);
}
```

---

## 📚 Risorse Utili

### Video Tutorials
- [Game Hacking Tutorial Series](https://www.youtube.com/ghh)
- [Pattern Scanning Explained](https://www.youtube.com/ps)
- [Cheat Engine Tutorial](https://www.youtube.com/ce)

### Documentazione
- [x64 Assembly Reference](https://www.intel.com/sdm)
- [Windows Memory Management](https://docs.microsoft.com/memory)
- [IDA Pro User Guide](https://www.hex-rays.com/ida)

### Community
- [UnknownCheats Forum](https://www.unknowncheats.me)
- [GuidedHacking](https://guidedhacking.com)
- [r/REGames](https://www.reddit.com/r/REGames)

---

## 🎓 Dopo aver Trovato i Pattern

1. **Aggiorna** `ACEPatterns.cs` con valori reali
2. **Test** con ACE Update 0.5
3. **Documenta** findings in questo file
4. **Commit** con messaggio: "Add ACE Update 0.5 patterns [TESTED]"
5. **Share** con community

---

## 💡 Tips Finali

✅ **Do:**
- Testa su multiple sessioni
- Documenta ogni pattern trovato
- Usa nomi descrittivi
- Aggiungi validation checks
- Version control dei pattern

❌ **Don't:**
- Non usare hardcoded addresses
- Non assumere offset fissi
- Non skippare validation
- Non modificare memoria del gioco
- Non usare in online multiplayer

---

**Buona fortuna con il reverse engineering! 🔍**

Per domande, apri una Issue o Discussion su GitHub.

_Guida v1.0 - Febbraio 2026_
