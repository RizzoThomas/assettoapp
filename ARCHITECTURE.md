# AssettoApp - Architecture Documentation

## Table of Contents
1. [System Architecture](#system-architecture)
2. [Technology Stack](#technology-stack)
3. [Module Descriptions](#module-descriptions)
4. [Data Flow](#data-flow)
5. [Technical Implementation Details](#technical-implementation-details)
6. [API Reference](#api-reference)
7. [Known Limitations](#known-limitations)

## System Architecture

AssettoApp follows a **modular, layered architecture** with clean separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                     UI Layer (WPF)                       │
│  ┌─────────────┐  ┌──────────────┐  ┌───────────────┐ │
│  │  MainWindow │  │ ViewModels   │  │  Converters   │ │
│  └─────────────┘  └──────────────┘  └───────────────┘ │
└───────────────────────────┬─────────────────────────────┘
                            │
        ┌───────────────────┴───────────────────┐
        │                                       │
┌───────▼─────────┐                   ┌────────▼────────┐
│  Application    │                   │   Setup         │
│  Logic Layer    │                   │   Generation    │
│  ┌────────────┐ │                   │   Layer         │
│  │ Telemetry  │ │                   │  ┌───────────┐ │
│  │ Analyzer   │ │                   │  │  Setup    │ │
│  └────────────┘ │                   │  │ Generator │ │
└───────┬─────────┘                   └────────┬────────┘
        │                                      │
        └──────────────┬───────────────────────┘
                       │
            ┌──────────▼──────────┐
            │   Core Layer        │
            │  ┌──────────────┐   │
            │  │   Models     │   │
            │  │ Interfaces   │   │
            │  └──────────────┘   │
            └──────────┬──────────┘
                       │
        ┌──────────────┴──────────────┐
        │                             │
┌───────▼─────────┐          ┌────────▼────────┐
│   Simulator     │          │   Simulator     │
│  Integration    │          │  Integration    │
│     (AC)        │          │    (ACE)        │
│  ┌───────────┐  │          │  ┌───────────┐  │
│  │  Shared   │  │          │  │ (Future   │  │
│  │  Memory   │  │          │  │  API)     │  │
│  │  Reader   │  │          │  └───────────┘  │
│  └───────────┘  │          └─────────────────┘
└────────┬────────┘
         │
    ┌────▼────┐
    │Assetto  │
    │ Corsa   │
    └─────────┘
```

### Design Principles

1. **Modular Design**: Each component has a single responsibility
2. **Dependency Injection**: Loose coupling through interfaces
3. **MVVM Pattern**: Clean separation between UI and logic
4. **Data-Driven**: No hardcoded values in setup generation
5. **Extensible**: Easy to add new simulators or analysis algorithms

## Technology Stack

### Core Technologies
- **Language**: C# 12
- **Framework**: .NET 8.0
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Target Platform**: Windows 10/11 (x64)

### Key Libraries
- `Microsoft.Extensions.DependencyInjection` - Dependency injection
- `LiveCharts.Wpf` - Charting (optional, for future visualizations)
- `System.IO.MemoryMappedFiles` - Shared memory access

### Architectural Patterns
- **MVVM** (Model-View-ViewModel)
- **Repository Pattern** (via ISimulatorConnector)
- **Strategy Pattern** (different connectors for AC/ACE)
- **Dependency Injection**

## Module Descriptions

### 1. AssettoApp.Core

**Purpose**: Foundation layer containing models and interfaces

**Key Components**:
- `GameType` enum - Simulator identification
- `TelemetryData` - Real-time telemetry model (60+ properties)
- `CarSetup` - Complete setup configuration
- `ISimulatorConnector` - Simulator integration interface
- `ITelemetryAnalyzer` - Analysis interface
- `ISetupGenerator` - Setup generation interface

**Dependencies**: None (pure domain logic)

### 2. AssettoApp.SimulatorIntegration

**Purpose**: Connects to simulators and reads telemetry data

**Key Components**:

#### ACSharedMemory.cs
Defines C# structs matching AC's shared memory layout:
- `ACPhysics` - Real-time physics data
- `ACGraphics` - Graphics and session info
- `ACStatic` - Static car/track information

#### ACSharedMemoryReader.cs
Reads from Windows memory-mapped files:
```csharp
MemoryMappedFile.OpenExisting("Local\\acpmf_physics")
```

#### ACESharedMemoryReader.cs
**NEW**: Dedicated reader for ACE with multi-pattern support:
```csharp
// Tries multiple naming patterns:
// 1. Local\acpmf_* (same as AC)
// 2. Local\acepmf_* (ACE-specific)
// 3. Local\ac2pmf_* (AC2 variant)
```
Automatically falls back to alternative patterns if primary fails.

#### AssettoCorساConnector.cs
Implements `ISimulatorConnector` for AC:
- Detects running AC process
- Maps shared memory to C# structs
- Converts to `TelemetryData` model

#### AssettoCorساEvoConnector.cs
**UPDATED**: Implements `ISimulatorConnector` for ACE:
- Detects running ACE process (AC2, AssettoCorsa2, assettocorsaevo)
- Uses `ACESharedMemoryReader` for telemetry access
- Tries multiple memory-mapped file naming patterns
- Reads currently selected car and track from game
- Converts to `TelemetryData` model with full telemetry support

**Dependencies**: 
- AssettoApp.Core
- System.IO.MemoryMappedFiles

### 3. AssettoApp.TelemetryAnalysis

**Purpose**: Analyzes telemetry data to extract insights

**Key Components**:

#### TelemetryAnalyzer.cs
Implements `ITelemetryAnalyzer`:

**Analysis Methods**:
1. `DetectDrivingStyle()` - Classifies driver behavior
   - Analyzes braking patterns
   - Throttle application smoothness
   - Steering input aggression
   
2. `AnalyzeTires()` - Tire behavior analysis
   - Temperature monitoring (overheating detection)
   - Pressure analysis
   - Temperature differentials (camber indication)
   
3. `AnalyzeSuspension()` - Suspension behavior
   - Travel analysis
   - Bottoming detection
   
4. `AnalyzeBalance()` - Vehicle balance
   - Understeer/oversteer detection
   - Slip angle analysis
   - Corner-by-corner tendency

**Dependencies**: AssettoApp.Core

### 4. AssettoApp.SetupGeneration

**Purpose**: Generates optimized setups from analysis results

**Key Components**:

#### SetupGenerator.cs
Implements `ISetupGenerator`:

**Optimization Algorithms**:

1. **Tire Pressures**:
   ```
   Base = 26 PSI
   If overheating: -1 PSI
   If temp < 75°C: +1 PSI
   If temp > 95°C: -1 PSI
   ```

2. **Suspension Springs**:
   ```
   Base = 80 N/mm (front), 85 N/mm (rear)
   If bottoming: +10 N/mm
   If aggressive style: +5 N/mm
   If smooth style: -5 N/mm
   ```

3. **Aerodynamics**:
   ```
   If oversteer: +1 rear wing
   If understeer: +1 front wing
   ```

4. **Differential**:
   ```
   Base power ramp = 60°
   If oversteer: -10° (less locking)
   If understeer: +10° (more rotation)
   ```

5. **Camber**:
   ```
   Base = -2.8° front, -2.5° rear
   If inner temp > outer +5°C: -0.3° (more negative)
   If outer temp > inner +5°C: +0.3° (less negative)
   ```

**Export Format**: Standard AC .INI format

**Dependencies**: AssettoApp.Core

### 5. AssettoApp.UI

**Purpose**: User interface and application orchestration

**Key Components**:

#### MVVM Structure:
- **ViewModels**: `MainViewModel` - Application state and commands
- **Views**: `MainWindow.xaml` - UI layout
- **Commands**: `RelayCommand` - Command pattern implementation
- **Converters**: Data binding helpers

#### MainViewModel.cs
Central application coordinator:
- Manages simulator connection
- Controls telemetry recording (100ms intervals)
- Triggers analysis and setup generation
- Handles user interactions

**UI Features**:
- Dark theme optimized for racing environments
- Real-time connection status
- Dropdown selectors for game/car/track
- Action buttons for workflow steps
- Results display panel

**Dependencies**: All other projects

## Data Flow

### 1. Connection Flow
```
User clicks "Connect"
    ↓
MainViewModel.ConnectToSimulator()
    ↓
ISimulatorConnector.ConnectAsync()
    ↓
ACSharedMemoryReader.Connect()
    ↓
MemoryMappedFile.OpenExisting()
    ↓
Status updated in UI
```

### 2. Telemetry Recording Flow
```
User clicks "Start Recording"
    ↓
Timer starts (100ms interval)
    ↓
ISimulatorConnector.ReadTelemetry()
    ↓
ACSharedMemoryReader.ReadPhysics()
ACSharedMemoryReader.ReadGraphics()
    ↓
Data converted to TelemetryData model
    ↓
Added to session data list
    ↓
Continues until "Stop Recording"
```

### 3. Analysis Flow
```
User clicks "Analyze Session"
    ↓
ITelemetryAnalyzer.AnalyzeSession(sessionData)
    ↓
DetectDrivingStyle()
AnalyzeTires()
AnalyzeSuspension()
AnalyzeBalance()
    ↓
TelemetryAnalysisResult created
    ↓
Results displayed in UI
```

### 4. Setup Generation Flow
```
User clicks "Generate Setup"
    ↓
ISetupGenerator.GenerateSetup(analysisResult)
    ↓
OptimizeTirePressures()
OptimizeSuspension()
OptimizeAerodynamics()
OptimizeDifferential()
OptimizeAlignment()
    ↓
CarSetup model created
    ↓
User clicks "Export Setup"
    ↓
ISetupGenerator.ExportSetupAsync()
    ↓
.INI file written to disk
```

## Technical Implementation Details

### Shared Memory Reading (Assetto Corsa)

AC exposes three memory-mapped files:

1. **Physics Data** (`Local\acpmf_physics`)
   - Size: ~1KB
   - Update Rate: 333Hz (every 3ms)
   - Contains: Vehicle dynamics, tire data, forces

2. **Graphics Data** (`Local\acpmf_graphics`)
   - Size: ~2KB
   - Update Rate: 60Hz
   - Contains: Session info, track conditions, position

3. **Static Data** (`Local\acpmf_static`)
   - Size: ~1KB
   - Update Rate: On session start
   - Contains: Car model, track name, max values

**Implementation**:
```csharp
_physicsMMF = MemoryMappedFile.OpenExisting(
    "Local\\acpmf_physics", 
    MemoryMappedFileRights.Read
);

using var accessor = _physicsMMF.CreateViewAccessor(
    0, 
    Marshal.SizeOf<ACPhysics>(), 
    MemoryMappedFileAccess.Read
);

accessor.Read(0, out ACPhysics physics);
```

### Thread Safety

- UI updates on main thread via data binding
- Telemetry reading on timer thread
- Thread-safe collections for session data

### Performance Considerations

- Memory-mapped files: Zero-copy reading
- Struct marshaling: Direct binary mapping
- 100ms sampling rate: Balance between data quality and performance
- Typical session: 5-10 laps ≈ 3000-6000 data points ≈ 3MB memory

## Known Limitations

### Assetto Corsa Limitations

1. **No Direct Downforce Values**: AC shared memory doesn't expose aerodynamic forces. Can be estimated from ride height and speed but currently returns 0.

2. **Limited Setup Validation**: App doesn't validate if generated values are within car-specific ranges. AC will clamp values on load.

3. **No Live Setup Application**: Cannot change setup while driving. Must export and manually load in AC.

4. **Windows Only**: Memory-mapped files are Windows-specific API.

### Assetto Corsa EVO Status

**UPDATED - February 2026**: ACE shared memory integration implemented!

1. **✅ Shared Memory Support**: `ACESharedMemoryReader` now connects to ACE telemetry
   - Tries multiple naming patterns: `acpmf_*`, `acepmf_*`, `ac2pmf_*`
   - Automatic fallback if one pattern fails
   
2. **✅ Full Telemetry Reading**: Reads all physics and graphics data
   - Vehicle dynamics (speed, RPM, steering, etc.)
   - Tire data (pressure, temperature, wear)
   - Lap tracking and session information
   - Currently selected car and track

3. **✅ Auto Car/Track Detection**: Reads car model and track from game memory

4. **⚠️ API Availability**: Depends on ACE exposing telemetry API
   - Implementation is ready and will attempt connection
   - If ACE doesn't expose data yet, connection will gracefully fail
   - Monitor ACE updates for telemetry API enablement

5. **❓ Unknown Setup Format**: ACE may use different setup file format than AC
   - Current exports use AC .INI format
   - May need adaptation when ACE setup format is documented

### General Limitations

1. **Rule-Based AI**: Setup generation uses predefined rules, not machine learning.

2. **No Historical Database**: Sessions are not persisted between app restarts.

3. **Single Session Analysis**: Cannot compare multiple sessions.

4. **Basic Visualizations**: Text-based results only, no charts (LiveCharts added for future use).

5. **English Only**: No internationalization support.

## Future Enhancements

### Planned Features
- Machine learning-based setup optimization
- Session history and comparison
- Telemetry visualization (charts)
- Multi-lap analysis with sector comparison
- Setup recommendations based on community data
- Support for additional simulators (iRacing, ACC, rFactor 2)

### ACE Integration Roadmap

**Status**: ✅ Core implementation complete!

**Completed**:
- ✅ ACE process detection (AC2, AssettoCorsa2, assettocorsaevo)
- ✅ Shared memory reader with multiple naming patterns
- ✅ Full telemetry data structures
- ✅ Car and track auto-detection
- ✅ Complete telemetry reading implementation

**Remaining**:
- [ ] Verify ACE exposes telemetry API (game-side requirement)
- [ ] Test with actual ACE when API is available
- [ ] Document ACE-specific setup format if different from AC
- [ ] Add ACE-specific parameters if needed
4. Add ACE-specific setup parameters
5. Handle differences from AC (if any)

## Troubleshooting

### Connection Fails
- Ensure simulator is running
- Start a session (practice/race)
- Check Windows firewall/antivirus
- Run AssettoApp as administrator

### No Data During Recording
- Verify connection status is green
- Ensure you're driving (not in menu)
- Check AC/ACE process is running

### Generated Setup Issues
- Values may be clamped by simulator
- Some cars have locked parameters
- Export and test incrementally

## Compilation Instructions

See [BUILD.md](BUILD.md) for complete build procedures.
