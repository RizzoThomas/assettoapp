namespace AssettoApp.Core.Models;

/// <summary>
/// Real-time telemetry data from the simulator
/// </summary>
public class TelemetryData
{
    // Basic information
    public DateTime Timestamp { get; set; }
    public GameType GameType { get; set; }
    
    // Vehicle dynamics
    public float SpeedKmh { get; set; }
    public float SteeringAngle { get; set; }
    public float Throttle { get; set; }  // 0.0 to 1.0
    public float Brake { get; set; }     // 0.0 to 1.0
    public float Clutch { get; set; }    // 0.0 to 1.0
    public int Gear { get; set; }
    public float RPM { get; set; }
    
    // Acceleration (G-forces)
    public float AccelerationX { get; set; }  // Lateral
    public float AccelerationY { get; set; }  // Vertical
    public float AccelerationZ { get; set; }  // Longitudinal
    
    // Aerodynamics
    public float AerodynamicLoad { get; set; }
    public float FrontDownforce { get; set; }
    public float RearDownforce { get; set; }
    
    // Tire data (FL, FR, RL, RR)
    public TireData[] Tires { get; set; } = new TireData[4];
    
    // Suspension travel (mm)
    public float[] SuspensionTravel { get; set; } = new float[4];
    
    // Slip angles (degrees)
    public float[] SlipAngles { get; set; } = new float[4];
    
    // Track conditions
    public float TrackTemperature { get; set; }
    public float AirTemperature { get; set; }
    public float TrackGrip { get; set; }  // 0.0 to 1.0
    
    // Vehicle position and rotation
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public float Pitch { get; set; }
    public float Roll { get; set; }
    public float Yaw { get; set; }
    
    // Understeer/Oversteer indicator
    public float SteeringRatio { get; set; }  // Positive = understeer, Negative = oversteer
    
    // Lap tracking
    public int CurrentLap { get; set; }
    public TimeSpan CurrentLapTime { get; set; }
    public TimeSpan? LastLapTime { get; set; }
    public TimeSpan? BestLapTime { get; set; }
    public int Sector { get; set; }  // 0-2 (Sector 1, 2, 3)
    
    // Lap quality
    public bool IsLapValid { get; set; } = true;
    public int TrackLimitViolations { get; set; }
    
    // Fuel
    public float Fuel { get; set; }  // Liters remaining
    public float MaxFuel { get; set; }  // Tank capacity
}

/// <summary>
/// Tire-specific telemetry data
/// </summary>
public class TireData
{
    public float Pressure { get; set; }           // PSI
    public float Temperature { get; set; }        // Celsius (average)
    public float InnerTemperature { get; set; }   // Celsius
    public float MiddleTemperature { get; set; }  // Celsius
    public float OuterTemperature { get; set; }   // Celsius
    public float Wear { get; set; }               // 0.0 (new) to 1.0 (worn)
    public float Load { get; set; }               // Newtons
    public float SlipRatio { get; set; }          // Longitudinal slip
}
