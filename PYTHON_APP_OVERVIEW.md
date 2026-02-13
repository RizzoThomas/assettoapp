# 🐍 Python Telemetry Application for Assetto Corsa EVO

## Overview

Complete **Python-based desktop application** for advanced telemetry monitoring and dynamic setup management for Assetto Corsa EVO.

This is a parallel implementation alongside the C#/.NET version, designed to be more accessible for the simracing community.

## ✨ Key Features

### Core Functionality

- **Real-Time Telemetry** - 100Hz data acquisition via shared memory
- **Lock-Up Detection** - Automatic wheel lock-up detection with customizable thresholds
- **Lap Comparison** - Compare current lap with best lap, sector deltas
- **Dynamic Setups** - Three preset strategies (Safe/Balanced/Aggressive)
- **Professional GUI** - Dark theme CustomTkinter interface
- **Observer Pattern** - Clean architecture for real-time updates
- **Comprehensive Logging** - Diagnostic system with loguru

### Technical Highlights

- **Language**: Python 3.10+
- **GUI**: CustomTkinter 5.2.1 (dark theme)
- **Architecture**: Observer pattern, modular design
- **Build**: PyInstaller single-file .exe with --admin flag
- **Lines of Code**: ~1,560
- **Documentation**: 16 KB of guides

## 📁 Project Structure

```
PythonTelemetryApp/
├── src/
│   ├── main.py                    # Entry point with logging setup
│   ├── telemetry/
│   │   ├── data_acquisition.py   # Shared memory reader
│   │   ├── observer.py            # Observer pattern implementation
│   │   └── lock_detector.py      # Lock-up detection algorithm
│   ├── analysis/
│   │   └── lap_comparison.py     # Lap analysis and comparison
│   ├── setup/
│   │   └── setup_manager.py      # Setup management (3 presets)
│   └── gui/
│       └── main_window.py        # CustomTkinter GUI (1200x800)
├── requirements.txt               # Python dependencies
├── build.py                       # Automated build script
├── README.md                      # User guide (7.5 KB)
└── BUILD_INSTRUCTIONS.md          # Build documentation (8.4 KB)
```

## 🚀 Quick Start

### Installation

```bash
cd PythonTelemetryApp
pip install -r requirements.txt
python src/main.py
```

### Build Executable

```bash
python build.py
# Output: dist/ACETelemetry.exe (~20-30 MB)
```

### Usage

1. Launch Assetto Corsa EVO
2. Enter a session (Practice/Race)
3. Run `ACETelemetry.exe`
4. Click "Connect to ACE"
5. Monitor telemetry and apply setups

## 🎨 GUI Features

**Main Window (1200x800)**:
- **Sidebar**: Connection status, setup buttons, controls
- **Telemetry Panel**: Speed, RPM, Gear, Throttle/Brake, Lap times
- **G-Force Display**: Lateral, Longitudinal, Vertical forces
- **Tire Temperatures**: Color-coded per wheel
- **Lock-Up Counters**: Per-wheel statistics

**Setup Strategies**:
- 🛡️ **Safe** - Conservative, high downforce, stable (green)
- ⚖️ **Balanced** - Optimal for most conditions (orange)
- 🔥 **Aggressive** - Low downforce, performance (red)

## 📊 Components

### Data Acquisition (`data_acquisition.py`)
- Reads ACE shared memory (`Local\acpmf_*`)
- Parses CarPhysics, WheelData, SessionData
- Compatible with AC/ACE structures
- Graceful fallback if API unavailable

### Lock-Up Detector (`lock_detector.py`)
- Compares vehicle speed vs wheel angular velocity
- Detectable when: slip_ratio > 0.15 and brake > 0.05
- Records events with XYZ coordinates
- Per-wheel counters and cooldown system

### Lap Comparison (`lap_comparison.py`)
- Buffers current lap data points
- Compares with best lap of session
- Calculates sector deltas
- Tracks valid vs invalid laps

### Setup Manager (`setup_manager.py`)
- Auto-detects ACE setup directory
- Three template profiles
- JSON format (adaptable to ACE format)
- Saves to `Documents/Assetto Corsa EVO/savedata/setups/{car}/`

### Observer Pattern (`observer.py`)
- `TelemetrySubject` - Publishes data changes
- `TelemetryObserver` - Interface for subscribers
- `TelemetryManager` - Coordinates updates
- Decoupled architecture, 100Hz update rate

## 🔧 Dependencies

```
customtkinter==5.2.1   # GUI framework
numpy==1.26.3          # Data processing
pandas==2.2.0          # Data analysis
loguru==0.7.2          # Logging
pywin32==306           # Windows APIs (shared memory)
Pillow==10.2.0         # Image support
pyinstaller==6.3.0     # Build tool
```

## 📚 Documentation

- **[README.md](PythonTelemetryApp/README.md)** - Complete user guide
- **[BUILD_INSTRUCTIONS.md](PythonTelemetryApp/BUILD_INSTRUCTIONS.md)** - Build documentation

## 🔐 Requirements

- **OS**: Windows 10/11 (64-bit)
- **Python**: 3.10+ (for development)
- **ACE**: Assetto Corsa EVO installed
- **Permissions**: Administrator (for shared memory access)

## 🛠️ Building

### Using Build Script

```bash
cd PythonTelemetryApp
python build.py
```

### Manual PyInstaller

```bash
pyinstaller --onefile --windowed --uac-admin --name ACETelemetry src/main.py
```

Output: `dist/ACETelemetry.exe`

## 🎯 Use Cases

### Track Day Analysis
- Record all laps automatically
- Compare with best lap
- Identify lock-up patterns
- Adjust setup based on data

### Setup Development
- Try Safe setup for initial laps
- Move to Balanced for optimization
- Use Aggressive for time attack
- Compare lap times between setups

### Telemetry Monitoring
- Real-time G-forces
- Tire temperature monitoring
- Lock-up awareness
- Lap-by-lap improvement tracking

## ⚠️ Compatibility Notes

**Shared Memory**:
- Uses `Local\acpmf_*` pattern (same as AC)
- Compatible with ACE if API is exposed
- Graceful fallback if unavailable
- Diagnostic logging for troubleshooting

**Setup Files**:
- JSON format (adaptable to ACE format)
- Auto-creates directory structure
- Saved to Documents folder
- Must be loaded manually in-game

## 🐛 Troubleshooting

**Cannot Connect to ACE**:
1. Ensure ACE is running and in active session
2. Check `ace_telemetry.log` for errors
3. Run as administrator
4. Verify shared memory API is available

**No Telemetry Data**:
1. Check ACE update version compatibility
2. Review logs for memory access errors
3. May require pattern updates for new ACE versions

**Setup Not Saving**:
1. Verify Documents folder path
2. Check write permissions
3. Review logs for file I/O errors

## 🔄 Development

### Running in Dev Mode

```bash
python src/main.py
```

### Testing Components

```python
# Test data acquisition
from src.telemetry.data_acquisition import ACEDataAcquisition
ace = ACEDataAcquisition()
if ace.connect():
    ace.read_telemetry()
    print(ace.get_car_physics())
```

### Adding Features

1. Update data structures in `data_acquisition.py`
2. Extend analysis in `analysis/` modules
3. Add GUI widgets in `main_window.py`
4. Update observer notifications if needed

## 📈 Metrics

- **Lines of Code**: 1,562
- **Python Files**: 13
- **Modules**: 5 (telemetry, analysis, setup, gui, main)
- **Documentation**: 16 KB (2 comprehensive guides)
- **Build Output**: ~20-30 MB (single .exe)

## 🤝 Comparison with C# Version

| Aspect | Python | C# |
|--------|--------|-----|
| **Language** | Python 3.10+ | C# 12 / .NET 8.0 |
| **GUI** | CustomTkinter | WPF |
| **Accessibility** | High (easier to mod) | Medium |
| **Performance** | Good (100Hz) | Excellent |
| **Build** | PyInstaller | dotnet publish |
| **Size** | ~25 MB | ~150 MB |

Both implementations share the same goals and can coexist.

## 📝 License

[Your License]

## 👥 Credits

Developed for the Assetto Corsa EVO community.

## 🆘 Support

For issues:
1. Check `ace_telemetry.log`
2. Review documentation
3. Open GitHub Issue

---

**Enjoy your telemetry analysis! 🏁**
