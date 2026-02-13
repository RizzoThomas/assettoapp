# AssettoApp - Telemetry Analysis & Setup Optimization Tool

## Overview

AssettoApp is a professional Windows desktop application designed for Assetto Corsa (AC) and Assetto Corsa EVO (ACE) that provides:
- **Two operational modes**: In-Game (with telemetry) and Offline (preset-based)
- Real-time telemetry analysis and data-driven setup optimization
- **Advanced lap tracking** with perfect lap detection, tire wear monitoring, and fuel consumption tracking
- **Persistent session history** - track your progress even when the game is closed
- Works both with and without the simulator running

## ✨ Key Features

### Dual Operation Modes

#### Mode A: In-Game (Game Running)
- **Live telemetry reading** via Shared Memory API
- **Real-time analysis** of driving behavior
- **Telemetry-optimized setups** based on actual data
- **Lap tracking** with perfect lap detection
- **Live tire wear and fuel monitoring**
- 60+ parameters analyzed every 100ms
- Automatic driving style detection

#### Mode B: Offline (Game Closed) 🆕
- **No simulator required** - works without AC/ACE running
- **Preset-based generation** using real car/track data
- **View session history** from previous driving sessions
- **Track statistics** including perfect laps, tire wear, best lap times
- Manual selection of car, track, and driving style preference
- Instant setup generation from physics-based knowledge base
- **10 real cars** across GT3, GT4, Formula, and Road classes
- **10 famous tracks** (Spa, Monza, Mugello, Imola, Silverstone, etc.)

### Advanced Telemetry Features 🆕
- **Perfect Lap Detection**: Automatically identifies laps with no track limit violations and consistent pace
- **Tire Wear Tracking**: Monitor tire degradation across each tire per lap and session
- **Fuel Consumption Tracking**: Track fuel usage per lap and estimate remaining laps
- **Session History**: All sessions are automatically saved and can be reviewed later
- **Live Statistics**: Real-time display of current lap, best lap, perfect laps count, tire wear %, and fuel remaining
- **Historical Data Access**: View past sessions even when the game is closed

### Automatic Game Detection
- Auto-detects running simulators (`assettocorsa.exe`, `assettocorsaevo.exe`)
- Suggests appropriate mode based on detection
- Manual override always available

### Comprehensive Features

- **Dual Simulator Support**: Works with both Assetto Corsa and Assetto Corsa EVO
- **Real-Time Telemetry Analysis**: Reads actual data from shared memory
- **Intelligent Setup Generation**: Creates data-driven setups based on:
  - Driving style detection (Smooth, Aggressive, Trail Braker, Early/Late Apex)
  - Track conditions (temperature, grip levels)
  - Tire behavior analysis
  - Suspension dynamics
  - Vehicle balance (understeer/oversteer tendency)
- **Comprehensive Setup Parameters**:
  - Tire pressures
  - Suspension (springs, dampers, ride height)
  - Aerodynamics (front/rear wings)
  - Differential settings
  - Anti-roll bars
  - Alignment (camber, toe, caster)
  - Brake balance
- **Setup Export**: Exports to .INI format compatible with Assetto Corsa

## System Requirements

- **Operating System**: Windows 10/11
- **.NET Runtime**: .NET 8.0 or later
- **Simulators**: Assetto Corsa or Assetto Corsa EVO installed
- **Recommended**: 8GB RAM, multi-core processor for smooth telemetry recording

## Installation & Building

See [ARCHITECTURE.md](ARCHITECTURE.md) for complete build instructions and technical details.

## Quick Start

### In-Game Mode (with Telemetry)
1. Launch Assetto Corsa or Assetto Corsa EVO
2. Start AssettoApp - it will auto-detect the running game
3. Click "Connect to Simulator"
4. Select your car and track
5. Click "Start Recording" and drive several laps
6. Click "Stop Recording" then "Analyze Session"
7. Click "Generate Setup" to create optimized settings based on your driving
8. Click "Export Setup" to save the configuration file

### Offline Mode (without Game Running)
1. Start AssettoApp without the simulator
2. Application enters Offline Mode automatically
3. Select game, car, track, and driving style preference
4. Click "Generate Preset Setup" - instant generation!
5. Review the physics-based setup parameters
6. Click "Export Setup" to save for later use

### Manual Mode Switching
- Use **"Game Open"** / **"Game Closed"** buttons to switch modes manually
- Click **"🔍 Auto-Detect Running Game"** button to automatically detect and switch

## Known Limitations

### Assetto Corsa EVO
**UPDATE**: ACE shared memory support has been implemented! The connector now:
- ✅ Attempts to connect via multiple shared memory patterns
- ✅ Reads telemetry data when ACE exposes it
- ✅ Automatically detects selected car and track
- ⚠️ **Note**: Full functionality depends on ACE exposing telemetry API (may require game updates)

### Assetto Corsa Original
Fully functional using official shared memory API. Some advanced aerodynamic data is not exposed by AC's telemetry system.

## Project Structure

```
AssettoApp/
├── AssettoApp.Core/              # Core models and interfaces
├── AssettoApp.SimulatorIntegration/  # AC/ACE connectivity
├── AssettoApp.TelemetryAnalysis/     # Data analysis engine
├── AssettoApp.SetupGeneration/       # Setup optimization
└── AssettoApp.UI/                    # WPF user interface
```

## Documentation

### 🇮🇹 Italian Documentation
- **[SETUP_DA_ZERO_IT.md](SETUP_DA_ZERO_IT.md)** - 🆕 Complete guide to clone and setup from scratch (Italian)
- **[QUICK_START_IT.md](QUICK_START_IT.md)** - 🆕 Quick reference card (Italian)
- **[BUILD_IT.md](BUILD_IT.md)** - Build instructions (Italian)
- **[ACE_INTEGRATION_IT.md](ACE_INTEGRATION_IT.md)** - ACE integration guide (Italian)
- **[OFFLINE_MODE_IT.md](OFFLINE_MODE_IT.md)** - Offline Mode documentation (Italian)
- **[IMPLEMENTAZIONE_IT.md](IMPLEMENTAZIONE_IT.md)** - Implementation summary (Italian)

### 🇬🇧 English Documentation
- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Complete technical architecture and API documentation
- **[BUILD.md](BUILD.md)** - Detailed build and deployment instructions
- **[PROJECT_OVERVIEW.md](PROJECT_OVERVIEW.md)** - Project file inventory and metrics

## License

[Specify your license]

## Author

Thomas Rizzo
