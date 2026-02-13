# Integrazione Assetto Corsa EVO - Guida Completa

## 🎮 Panoramica

AssettoApp ora supporta la lettura telemetria da **Assetto Corsa EVO** tramite shared memory, proprio come per AC originale!

## ✨ Funzionalità Implementate

### 1. Rilevamento Automatico Auto Selezionata
Quando ti connetti ad ACE, l'applicazione legge automaticamente:
- ✅ **Auto correntemente selezionata** nel gioco
- ✅ **Tracciato correntemente selezionato** nel gioco
- ✅ Tutti i dati telemetrici in tempo reale

### 2. Lettura Telemetria Completa
L'app legge oltre 60 parametri ogni 100ms:
- Velocità, RPM, marcia, sterzo
- Accelerazioni G (laterali, longitudinali, verticali)
- **Gomme**: pressione, temperatura (interna/media/esterna), usura, carico
- **Sospensioni**: escursione, angoli di deriva
- **Condizioni pista**: temperatura asfalto/aria, grip
- **Lap tracking**: tempi giro, settori, validità giro
- **Carburante**: livello carburante rimanente

### 3. Generazione Setup Ottimizzati
Una volta raccolti i dati, l'app genera setup personalizzati basati su:
- Il tuo stile di guida (rilevato automaticamente)
- Comportamento delle gomme
- Bilanciamento del veicolo
- Condizioni della pista

## 🚀 Come Usare

### Passo 1: Avvia Assetto Corsa EVO
1. Apri Steam e avvia **Assetto Corsa EVO**
2. Entra in una sessione (Pratica, Gara, ecc.)
3. Seleziona la tua auto e il tracciato preferito

### Passo 2: Connetti AssettoApp
1. Avvia **AssettoApp**
2. L'app rileverà automaticamente ACE in esecuzione
3. Clicca su **"🔍 Auto-Detect Running Game"** se necessario
4. Clicca su **"Connect to Simulator"**

### Passo 3: Visualizza Auto e Tracciato
Dopo la connessione, vedrai:
- **Auto selezionata**: Il nome dell'auto che stai usando in ACE
- **Tracciato selezionato**: Il nome del circuito corrente

### Passo 4: Registra Telemetria
1. Clicca **"Start Recording"**
2. Guida alcuni giri (almeno 3-5 giri puliti)
3. Clicca **"Stop Recording"**

### Passo 5: Analizza e Genera Setup
1. Clicca **"Analyze Session"** - l'app analizza il tuo stile di guida
2. Clicca **"Generate Setup"** - crea un setup ottimizzato
3. Clicca **"Export Setup"** - salva il file .INI

### Passo 6: Applica il Setup
1. Copia il file .INI esportato
2. Incollalo nella cartella setup di ACE
3. Carica il setup nel gioco

## 🔧 Dettagli Tecnici

### Shared Memory API
ACE usa un sistema simile ad AC originale con memory-mapped files. L'applicazione cerca automaticamente tre pattern:
- `Local\acpmf_physics`, `Local\acpmf_graphics`, `Local\acpmf_static`
- `Local\acepmf_physics`, `Local\acepmf_graphics`, `Local\acepmf_static`
- `Local\ac2pmf_physics`, `Local\ac2pmf_graphics`, `Local\ac2pmf_static`

### Nomi Processi Supportati
L'app rileva ACE cercando questi processi:
- `AC2.exe`
- `AssettoCorsa2.exe`
- `assettocorsaevo.exe`

## 📊 Esempio di Utilizzo

```
1. Apri ACE → Seleziona Ferrari 296 GTB @ Spa-Francorchamps
2. Avvia AssettoApp → Auto-rilevamento: "Assetto Corsa EVO detected"
3. Clicca "Connect" → Stato: "Connected to AssettoCorساEvo"
4. Visualizza: 
   - Auto selezionata: "ferrari_296_gtb"
   - Tracciato: "spa_francorchamps"
5. Registra 5 giri
6. Analizza → Stile rilevato: "Aggressive"
7. Genera setup → Ottimizzato per:
   - Pressioni gomme: 27.5 PSI (basato su temperature)
   - Camber: -3.2° anteriore (basato su temp. gomme)
   - Alettone posteriore: +2 (per bilanciamento)
```

## ⚠️ Note Importanti

### Stato API ACE
- **Febbraio 2026**: ACE potrebbe non esporre ancora completamente l'API telemetria
- L'implementazione è pronta e tenterà la connessione
- Se ACE non espone i dati, vedrai "Failed to connect"
- Monitora gli aggiornamenti di ACE che potrebbero abilitare la telemetria

### Compatibilità
- ✅ Windows 10/11 (richiesto per shared memory)
- ✅ .NET 8.0 o superiore
- ✅ ACE deve essere in esecuzione con una sessione attiva

### Risoluzione Problemi

**Problema**: "Failed to connect"
- **Soluzione 1**: Assicurati che ACE sia in esecuzione con una sessione attiva (non solo nel menu)
- **Soluzione 2**: Riavvia ACE e riprova
- **Soluzione 3**: ACE potrebbe non esporre ancora l'API - attendi aggiornamenti

**Problema**: Auto/Tracciato vuoti
- **Soluzione**: Entra in una sessione di guida, non fermarti nel menu principale

**Problema**: Telemetria non si aggiorna
- **Soluzione**: Riconnetti l'applicazione dopo aver caricato la sessione in ACE

## 🎯 Migliori Pratiche

1. **Giri Puliti**: Registra almeno 3-5 giri senza track limits per analisi accurate
2. **Condizioni Stabili**: Evita cambi di condizioni meteo durante la registrazione
3. **Riscaldamento Gomme**: Completa 1-2 giri di riscaldamento prima di registrare
4. **Setup Base**: Parti da un setup di base del gioco, poi ottimizza

## 📈 Prossimi Sviluppi

- [ ] Supporto setup specifici per ACE (se formato diverso da AC)
- [ ] Database setup community per auto ACE
- [ ] Confronto setup multipli
- [ ] Visualizzazioni grafiche telemetria in tempo reale

## 💡 Suggerimenti

- **Auto GT3**: Focus su bilanciamento aerodinamico e pressioni gomme
- **Auto Road**: Più importante ottimizzare sospensioni e differenziale
- **Tracciati veloci** (Monza, Spa): Alettoni bassi, setup efficienza
- **Tracciati tecnici** (Monaco, Brands Hatch): Alettoni alti, setup grip

## 🤝 Supporto

Problemi o domande? Apri una issue su GitHub!

---

**Buona guida! 🏁**
