using AssettoApp.Core.Interfaces;
using AssettoApp.Core.Models;
using AssettoApp.SimulatorIntegration.SharedMemory;
using System.Diagnostics;

namespace AssettoApp.SimulatorIntegration.Connectors;

/// <summary>
/// Connector for Assetto Corsa (original game)
/// Uses shared memory to read telemetry data
/// </summary>
public class AssettoCorساConnector : ISimulatorConnector
{
    private readonly ACSharedMemoryReader _memoryReader;
    private ACStatic? _staticInfo;

    public GameType SupportedGame => GameType.AssettoCorsaOriginal;

    public AssettoCorساConnector()
    {
        _memoryReader = new ACSharedMemoryReader();
    }

    public bool IsSimulatorRunning()
    {
        // Check if AC process is running
        var processes = Process.GetProcessesByName("ac");
        return processes.Length > 0;
    }

    public async Task<bool> ConnectAsync()
    {
        await Task.Delay(100); // Small delay to ensure process is ready
        
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
            GameType = GameType.AssettoCorsaOriginal,
            
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
            
            // Aerodynamics - AC doesn't expose direct downforce values in shared memory
            // We can estimate from ride height and speed, but marking as unavailable for now
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
        
        // In a real implementation, this would scan the AC installation directory
        // For now, return a sample list of popular AC cars
        return new List<string>
        {
            "ferrari_458_gt2",
            "mercedes_sls_gt3",
            "bmw_z4_gt3",
            "audi_r8_lms",
            "porsche_911_gt3_r",
            "lamborghini_huracan_gt3",
            "mclaren_650s_gt3"
        };
    }

    public async Task<List<string>> GetAvailableTracksAsync()
    {
        await Task.CompletedTask;
        
        // In a real implementation, this would scan the AC installation directory
        // Sample list of popular AC tracks
        return new List<string>
        {
            "spa",
            "monza",
            "mugello",
            "imola",
            "nurburgring",
            "silverstone",
            "brands_hatch",
            "barcelona",
            "red_bull_ring"
        };
    }
}
