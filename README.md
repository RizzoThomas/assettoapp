# AssettoApp - Telemetry Analysis & Setup Optimization Tool

## Overview

AssettoApp is a professional Windows desktop application designed for Assetto Corsa (AC) and Assetto Corsa EVO (ACE) that provides:
- **Two operational modes**: In-Game (with telemetry) and Offline (preset-based)
- Real-time telemetry analysis and data-driven setup optimization
- Works both with and without the simulator running

## ✨ Key Features

### Dual Operation Modes

#### Mode A: In-Game (Game Running)
- **Live telemetry reading** via Shared Memory API
- **Real-time analysis** of driving behavior
- **Telemetry-optimized setups** based on actual data
- 60+ parameters analyzed every 100ms
- Automatic driving style detection

#### Mode B: Offline (Game Closed) 🆕
- **No simulator required** - works without AC/ACE running
- **Preset-based generation** using real car/track data
- Manual selection of car, track, and driving style preference
- Instant setup generation from physics-based knowledge base
- **10 real cars** across GT3, GT4, Formula, and Road classes
- **10 famous tracks** (Spa, Monza, Mugello, Imola, Silverstone, etc.)

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
**IMPORTANT**: ACE is in early access and doesn't yet provide public telemetry API access. The ACE connector is implemented as a framework but will not function until Kunos releases telemetry support.

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

- **[OFFLINE_MODE_IT.md](OFFLINE_MODE_IT.md)** - Complete Italian documentation for Offline Mode (Mode B)
- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Complete technical architecture and API documentation
- **[BUILD.md](BUILD.md)** - Detailed build and deployment instructions
- **[IMPLEMENTAZIONE_IT.md](IMPLEMENTAZIONE_IT.md)** - Italian implementation summary
- **[PROJECT_OVERVIEW.md](PROJECT_OVERVIEW.md)** - Project file inventory and metrics

## License

[Specify your license]

## Author

Thomas Rizzo
