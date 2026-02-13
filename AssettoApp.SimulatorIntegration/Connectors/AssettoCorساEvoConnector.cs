using AssettoApp.Core.Interfaces;
using AssettoApp.Core.Models;
using AssettoApp.SimulatorIntegration.SharedMemory;
using System.Diagnostics;

namespace AssettoApp.SimulatorIntegration.Connectors;

/// <summary>
/// Connector for Assetto Corsa EVO
/// Implements telemetry reading via shared memory API
/// ACE uses similar approach to AC original with memory-mapped files
/// </summary>
public class AssettoCorساEvoConnector : ISimulatorConnector
{
    private readonly ACESharedMemoryReader _memoryReader;
    private ACStatic? _staticInfo;

    public GameType SupportedGame => GameType.AssettoCorساEvo;

    public AssettoCorساEvoConnector()
    {
        _memoryReader = new ACESharedMemoryReader();
    }

    public bool IsSimulatorRunning()
    {
        // Check if ACE process is running
        // Check multiple possible process names
        var processes = Process.GetProcessesByName("AC2");
        if (processes.Length == 0)
        {
            processes = Process.GetProcessesByName("AssettoCorsa2");
        }
        if (processes.Length == 0)
        {
            processes = Process.GetProcessesByName("assettocorsaevo");
        }
        return processes.Length > 0;
    }

    public async Task<bool> ConnectAsync()
    {
        await Task.Delay(100); // Small delay to ensure process is ready
        
        // Try to connect to ACE shared memory
        // The memory reader tries multiple naming patterns
        if (_memoryReader.Connect())
        {
            _staticInfo = _memoryReader.ReadStatic();
            return _staticInfo != null;
        }
        
        return false;
    }

    public void Disconnect()
    {
        _memoryReader.Disconnect();
        _staticInfo = null;
    }

    public TelemetryData? ReadTelemetry()
    {
        var physics = _memoryReader.ReadPhysics();
        var graphics = _memoryReader.ReadGraphics();
        
        if (physics == null || graphics == null)
            return null;

        var p = physics.Value;
        var g = graphics.Value;

        var telemetry = new TelemetryData
        {
            Timestamp = DateTime.Now,
            GameType = GameType.AssettoCorساEvo,
            
            // Vehicle dynamics
            SpeedKmh = p.SpeedKmh,
            SteeringAngle = p.SteerAngle,
            Throttle = p.Gas,
            Brake = p.Brake,
            Clutch = p.Clutch,
            Gear = p.Gear,
            RPM = p.Rpms,
            
            // Accelerations (convert to G-forces)
            AccelerationX = p.AccG[0],
            AccelerationY = p.AccG[1],
            AccelerationZ = p.AccG[2],
            
            // Aerodynamics - may not be directly exposed in shared memory
            AerodynamicLoad = 0,
            FrontDownforce = 0,
            RearDownforce = 0,
            
            // Track conditions
            TrackTemperature = p.RoadTemp,
            AirTemperature = p.AirTemp,
            TrackGrip = g.SurfaceGrip,
            
            // Position and rotation
            PositionX = g.CarCoordinates[0],
            PositionY = g.CarCoordinates[1],
            PositionZ = g.CarCoordinates[2],
            Pitch = p.Pitch,
            Roll = p.Roll,
            Yaw = p.Heading,
            
            // Suspension travel
            SuspensionTravel = p.SuspensionTravel,
            
            // Slip angles
            SlipAngles = p.SlipAngle,
            
            // Lap tracking
            CurrentLap = g.CompletedLaps + 1,
            CurrentLapTime = TimeSpan.FromMilliseconds(g.iCurrentTime),
            LastLapTime = g.iLastTime > 0 ? TimeSpan.FromMilliseconds(g.iLastTime) : null,
            BestLapTime = g.iBestTime > 0 ? TimeSpan.FromMilliseconds(g.iBestTime) : null,
            Sector = g.CurrentSectorIndex,
            IsLapValid = g.IsValidLap == 1,
            
            // Fuel
            Fuel = p.Fuel,
            MaxFuel = _staticInfo?.MaxFuel ?? 0,
            
            // Calculate understeer/oversteer tendency
            SteeringRatio = CalculateSteeringRatio(p)
        };

        // Populate tire data
        for (int i = 0; i < 4; i++)
        {
            telemetry.Tires[i] = new TireData
            {
                Pressure = p.WheelPressure[i],
                Temperature = p.TyreCoreTemperature[i],
                InnerTemperature = p.TyreTempI[i],
                MiddleTemperature = p.TyreTempM[i],
                OuterTemperature = p.TyreTempO[i],
                Wear = p.TyreWear[i],
                Load = p.WheelLoad[i],
                SlipRatio = p.WheelSlip[i]
            };
        }

        return telemetry;
    }

    private float CalculateSteeringRatio(ACPhysics physics)
    {
        // Simple estimation of understeer/oversteer based on slip angles
        // Positive = understeer, Negative = oversteer
        if (physics.SlipAngle == null || physics.SlipAngle.Length < 4)
            return 0;

        float frontSlip = (Math.Abs(physics.SlipAngle[0]) + Math.Abs(physics.SlipAngle[1])) / 2f;
        float rearSlip = (Math.Abs(physics.SlipAngle[2]) + Math.Abs(physics.SlipAngle[3])) / 2f;
        
        return frontSlip - rearSlip;
    }

    public async Task<List<string>> GetAvailableCarsAsync()
    {
        await Task.CompletedTask;
        
        // If connected and have static info, return currently selected car
        if (_staticInfo != null && !string.IsNullOrEmpty(_staticInfo.Value.CarModel))
        {
            return new List<string> { _staticInfo.Value.CarModel };
        }
        
        // Otherwise return ACE car list (updated as content is released)
        return new List<string>
        {
            "alfa_romeo_giulia_gta",
            "ferrari_296_gtb", 
            "porsche_992_gt3",
            "bmw_m4_csl",
            "mercedes_amg_gt_black_series",
            "ferrari_488_challenge_evo",
            "lamborghini_huracan_st_evo2",
            "porsche_911_gt3_cup"
        };
    }

    public async Task<List<string>> GetAvailableTracksAsync()
    {
        await Task.CompletedTask;
        
        // If connected and have static info, return currently selected track
        if (_staticInfo != null && !string.IsNullOrEmpty(_staticInfo.Value.Track))
        {
            return new List<string> { _staticInfo.Value.Track };
        }
        
        // Otherwise return ACE track list (update as content is released)
        return new List<string>
        {
            "nurburgring_nordschleife",
            "brands_hatch",
            "spa_francorchamps",
            "imola",
            "monza",
            "barcelona",
            "laguna_seca"
        };
    }
    
    /// <summary>
    /// Get the currently selected car from the game
    /// Returns null if not connected or car info not available
    /// </summary>
    public string? GetCurrentCar()
    {
        return _staticInfo?.CarModel;
    }
    
    /// <summary>
    /// Get the currently selected track from the game
    /// Returns null if not connected or track info not available
    /// </summary>
    public string? GetCurrentTrack()
    {
        return _staticInfo?.Track;
    }
}
