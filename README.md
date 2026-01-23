# AssettoApp - Telemetry Analysis & Setup Optimization Tool

## Overview

AssettoApp is a professional Windows desktop application designed for Assetto Corsa (AC) and Assetto Corsa EVO (ACE) that analyzes real-time telemetry data and generates optimized car setups based on driving style, track conditions, and vehicle behavior.

## Features

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

1. Launch Assetto Corsa or Assetto Corsa EVO
2. Start AssettoApp
3. Select your game and click "Connect to Simulator"
4. Select your car and track
5. Click "Start Recording" and drive several laps
6. Click "Stop Recording" then "Analyze Session"
7. Click "Generate Setup" to create optimized settings
8. Click "Export Setup" to save the configuration file

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

- [ARCHITECTURE.md](ARCHITECTURE.md) - Complete technical architecture and API documentation
- [BUILD.md](BUILD.md) - Detailed build and deployment instructions

## License

[Specify your license]

## Author

Thomas Rizzo
