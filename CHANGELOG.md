# Changelog - AssettoApp

Tutte le modifiche importanti al progetto saranno documentate in questo file.

## [1.0.0] - 2024-01-23

### Aggiunto
- ✅ Implementazione completa architettura modulare a 5 progetti
- ✅ Integrazione con Assetto Corsa via Shared Memory
- ✅ Lettura telemetria in tempo reale (60+ parametri)
- ✅ Analisi intelligente dati telemetrici:
  - Rilevamento stile di guida (5 stili)
  - Analisi gomme (temperatura, pressione, usura)
  - Analisi sospensioni (escursione, battute a fondo)
  - Analisi bilanciamento (sottosterzo/sovrasterzo)
  - Analisi condizioni pista
- ✅ Generazione setup ottimizzati (40+ parametri):
  - Pressioni gomme
  - Molle e ammortizzatori
  - Camber, toe, caster
  - Differenziale
  - Ali aerodinamiche
  - Barre antirollio
  - Bilanciamento freni
- ✅ Interfaccia WPF professionale con tema scuro
- ✅ Export setup in formato .INI compatibile con AC
- ✅ Framework per Assetto Corsa EVO (in attesa API)
- ✅ Documentazione completa:
  - README.md (inglese)
  - BUILD.md (istruzioni build dettagliate)
  - BUILD_IT.md (guida rapida italiano)
  - ARCHITECTURE.md (dettagli tecnici)
  - IMPLEMENTAZIONE_IT.md (riepilogo italiano)
  - PROJECT_OVERVIEW.md (inventario file)
- ✅ Script di build automatici (build.bat, build.sh)
- ✅ Gestione errori e feedback utente
- ✅ Pattern MVVM con Dependency Injection
- ✅ .gitignore configurato

### Miglioramenti Tecnici v1.0.0
- 🔧 Risolto warning async in MainViewModel (UpdateAvailableItems)
- 🔧 Aggiunto controllo piattaforma per Windows-only APIs
- 🔧 Aggiunte informazioni versione in AssemblyInfo
- 🔧 Configurato PublishReadyToRun per startup più veloce
- 🔧 Abilitato TieredCompilation per performance migliori

### Limitazioni Note
- ⚠️ ACE: Nessuna API telemetria disponibile (early access)
- ⚠️ AC: Valori downforce non esposti in shared memory
- ⚠️ Solo Windows (memory-mapped files)
- ⚠️ Ottimizzazione basata su regole (non ML)

### Requisiti
- Windows 10/11 (64-bit)
- .NET 8.0 SDK (per build)
- .NET 8.0 Runtime (se framework-dependent)
- Assetto Corsa installato e funzionante

### Prossimi Sviluppi Pianificati
- [ ] Grafici telemetria in tempo reale (LiveCharts)
- [ ] Storico sessioni con database locale
- [ ] Confronto multi-sessione
- [ ] Importazione setup da community
- [ ] Machine learning per ottimizzazione
- [ ] Supporto ACE quando API disponibile
- [ ] Analisi settori giro per giro
- [ ] Esportazione dati telemetria in CSV/JSON
- [ ] Preset setup predefiniti per circuiti popolari

---

## Formato del Changelog

Questo progetto segue [Semantic Versioning](https://semver.org/):
- **MAJOR** (X.0.0): Cambiamenti incompatibili con versioni precedenti
- **MINOR** (1.X.0): Nuove funzionalità retrocompatibili
- **PATCH** (1.0.X): Bug fix e piccoli miglioramenti

Categorie di cambiamenti:
- `Aggiunto` - Nuove funzionalità
- `Modificato` - Cambiamenti a funzionalità esistenti
- `Deprecato` - Funzionalità che saranno rimosse
- `Rimosso` - Funzionalità rimosse
- `Risolto` - Bug fix
- `Sicurezza` - Vulnerabilità corrette
