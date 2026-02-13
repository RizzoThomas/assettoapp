# 🏎️ ACE Telemetry - Python Application

## Overview

Advanced telemetry monitoring and dynamic setup management application for **Assetto Corsa EVO (ACE)**.

### Features

✅ **Real-Time Data Acquisition**
- Shared memory reading from ACE
- 100Hz update rate
- Car physics, wheel data, session information

✅ **Lock-Up Detection**
- Automatic wheel lock-up detection
- Customizable thresholds (slip ratio, brake input)
- Per-wheel statistics and event logging

✅ **Lap Analysis**
- Lap-by-lap comparison
- Sector delta calculations
- Best lap tracking

✅ **Dynamic Setup Management**
- Three preset strategies: Safe, Balanced, Aggressive
- Auto-save to ACE setup directory
- Track and car-specific configurations

✅ **Modern GUI**
- Dark theme with CustomTkinter
- Real-time telemetry display
- G-Force visualization
- Tire temperature monitoring
- Lock-up counters

✅ **Observer Pattern**
- Decoupled architecture
- Efficient telemetry updates
- Extensible design

✅ **Comprehensive Logging**
- Loguru-based logging system
- Rotation and retention
- Debug and production modes

## Installation

### Prerequisites

- **Windows 10/11** (64-bit)
- **Python 3.10+**
- **Assetto Corsa EVO** installed

### Setup

1. **Clone the repository**:
   ```bash
   cd PythonTelemetryApp
   ```

2. **Install dependencies**:
   ```bash
   pip install -r requirements.txt
   ```

3. **Run the application**:
   ```bash
   python src/main.py
   ```

## Usage

### 1. Connect to ACE

1. Launch Assetto Corsa EVO
2. Enter a session (Practice, Race, etc.)
3. The application will auto-connect
4. Or click "Connect to ACE" button

### 2. Monitor Telemetry

The main window displays:
- **Speed, RPM, Gear** - Current vehicle state
- **Throttle/Brake** - Input percentages
- **G-Forces** - Lateral, Longitudinal, Vertical
- **Tire Temperatures** - Color-coded per wheel
- **Lap Times** - Current and best lap
- **Lock-Ups** - Per-wheel counter

### 3. Setup Strategies

Click one of three setup buttons:

- **🛡️ Safe Setup** - Conservative, high downforce, stable
- **⚖️ Balanced Setup** - Optimal for most conditions
- **🔥 Aggressive Setup** - Low downforce, performance-oriented

Setup files are saved to:
```
Documents/Assetto Corsa EVO/savedata/setups/{car}/{track}_{type}.json
```

### 4. Lock-Up Analysis

Lock-ups are automatically detected when:
- Wheel slip ratio > 0.15 (configurable)
- Brake input > 5% (configurable)

Each event logs:
- Timestamp
- Wheel position (FL, FR, RL, RR)
- Track coordinates (X, Y, Z)
- Slip ratio and brake input
- Vehicle speed

### 5. Lap Comparison

The system:
- Records each lap automatically
- Compares with best lap
- Calculates sector deltas
- Tracks valid vs. invalid laps

## Architecture

```
PythonTelemetryApp/
├── src/
│   ├── main.py                    # Entry point
│   ├── telemetry/
│   │   ├── data_acquisition.py   # Shared memory reader
│   │   ├── observer.py            # Observer pattern
│   │   └── lock_detector.py      # Lock-up detection
│   ├── analysis/
│   │   └── lap_comparison.py     # Lap analysis
│   ├── setup/
│   │   └── setup_manager.py      # Setup management
│   └── gui/
│       └── main_window.py        # CustomTkinter GUI
├── requirements.txt
└── README.md
```

### Design Patterns

**Observer Pattern**:
- `TelemetrySubject` - Publishes data changes
- `TelemetryObserver` - Subscribes to updates
- Decouples data acquisition from UI

**Data Classes**:
- `CarPhysics` - Vehicle state
- `WheelData` - Per-wheel telemetry
- `SessionData` - Session information
- `LockUpEvent` - Lock-up occurrences

## Configuration

### Lock-Up Detector

Edit thresholds in code or GUI:
```python
lock_detector = LockUpDetector(
    slip_threshold=0.15,    # 15% slip ratio
    brake_threshold=0.05    # 5% brake input
)
```

### Setup Templates

Modify setup parameters in `src/setup/setup_manager.py`:
- Tire pressures
- Suspension settings
- Aerodynamics (wings)
- Differential
- Brake balance

## Building Executable

### Create Standalone .exe

```bash
pyinstaller --onefile --windowed --name ACETelemetry \
    --add-data "src;src" \
    --icon=icon.ico \
    --uac-admin \
    src/main.py
```

### Flags Explained

- `--onefile` - Single executable file
- `--windowed` - No console window
- `--name` - Output filename
- `--add-data` - Include source files
- `--uac-admin` - Request admin privileges (for shared memory access)

Output in: `dist/ACETelemetry.exe`

### Alternative Build Script

Use the provided script:
```bash
python build.py
```

## Troubleshooting

### "Not Connected" - Cannot connect to ACE

**Causes**:
1. ACE is not running
2. Not in an active session (menu doesn't provide telemetry)
3. Shared memory names changed in ACE update

**Solutions**:
1. Launch ACE and enter a session
2. Check logs: `ace_telemetry.log`
3. Verify process name in Task Manager
4. May need to update memory-mapped file names for new ACE versions

### No Telemetry Data

**Causes**:
1. ACE API changed
2. Shared memory structure mismatch

**Solutions**:
1. Check `ace_telemetry.log` for errors
2. May require reverse engineering for ACE Update 0.5+
3. See C# implementation in repo for reference

### Setup Files Not Saving

**Causes**:
1. Incorrect ACE directory
2. Permissions issue

**Solutions**:
1. Check setup directory path in logs
2. Run as administrator
3. Verify Documents folder location

### High CPU Usage

**Causes**:
1. 100Hz update rate

**Solutions**:
1. Reduce update frequency in `_telemetry_loop()`
2. Change `time.sleep(0.01)` to `time.sleep(0.02)` (50Hz)

## Logging

Logs are written to: `ace_telemetry.log`

**Log Levels**:
- DEBUG - Detailed diagnostic information
- INFO - General information
- WARNING - Warning messages (e.g., lock-ups)
- ERROR - Error messages

**Configuration** in `src/main.py`:
```python
logger.add(
    "ace_telemetry.log",
    rotation="10 MB",
    retention="7 days",
    level="DEBUG"
)
```

## Development

### Running in Development

```bash
cd PythonTelemetryApp
python src/main.py
```

### Testing Components

Test individual modules:
```python
# Test data acquisition
from src.telemetry.data_acquisition import ACEDataAcquisition

ace = ACEDataAcquisition()
if ace.connect():
    ace.read_telemetry()
    print(ace.get_car_physics())
```

### Adding Features

To add new telemetry displays:
1. Update `data_acquisition.py` with new data fields
2. Extend `CarPhysics`, `WheelData`, or `SessionData` dataclasses
3. Add widgets in `main_window.py`
4. Update observer pattern if needed

## API Reference

### ACEDataAcquisition

```python
ace = ACEDataAcquisition()
ace.connect() -> bool
ace.read_telemetry() -> bool
ace.get_car_physics() -> CarPhysics
ace.get_wheel_data(index: int) -> WheelData
ace.get_session_data() -> SessionData
ace.disconnect()
```

### LockUpDetector

```python
detector = LockUpDetector(slip_threshold=0.15, brake_threshold=0.05)
events = detector.check_lock_ups(car_physics, wheels)
detector.get_lock_up_count() -> int
detector.set_thresholds(slip_threshold, brake_threshold)
detector.clear_events()
```

### SetupManager

```python
manager = SetupManager()
manager.set_track_and_car(track, car)
manager.apply_setup("Safe" | "Balanced" | "Aggressive") -> bool
manager.get_available_setups(car) -> list
manager.load_setup(name, car) -> dict
```

## License

[Your License]

## Credits

Developed for the Assetto Corsa EVO community.

## Support

For issues or questions:
1. Check logs: `ace_telemetry.log`
2. Review documentation
3. Open GitHub Issue

---

**Enjoy racing! 🏁**
