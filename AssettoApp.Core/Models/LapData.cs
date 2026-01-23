namespace AssettoApp.Core.Models;

/// <summary>
/// Data for a single lap
/// </summary>
public class LapData
{
    public int LapNumber { get; set; }
    public TimeSpan LapTime { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    // Lap quality metrics
    public bool IsValid { get; set; } = true;  // No track limits violations
    public bool IsPerfectLap { get; set; }     // No mistakes, optimal racing line
    public int TrackLimitViolations { get; set; }
    
    // Sector times
    public TimeSpan? Sector1Time { get; set; }
    public TimeSpan? Sector2Time { get; set; }
    public TimeSpan? Sector3Time { get; set; }
    
    // Tire data at lap start and end
    public TireWearSnapshot? TireWearStart { get; set; }
    public TireWearSnapshot? TireWearEnd { get; set; }
    
    // Fuel data
    public float FuelAtStart { get; set; }
    public float FuelAtEnd { get; set; }
    public float FuelConsumed => FuelAtStart - FuelAtEnd;
    
    // Average conditions during lap
    public float AverageSpeed { get; set; }
    public float MaxSpeed { get; set; }
    public float AverageThrottle { get; set; }
    public float AverageBrake { get; set; }
    
    // Track conditions
    public float TrackTemperature { get; set; }
    public float AirTemperature { get; set; }
    public float TrackGrip { get; set; }
}

/// <summary>
/// Snapshot of tire wear at a specific point in time
/// </summary>
public class TireWearSnapshot
{
    public DateTime Timestamp { get; set; }
    
    public float FrontLeftWear { get; set; }      // 0.0 (new) to 1.0 (worn)
    public float FrontRightWear { get; set; }
    public float RearLeftWear { get; set; }
    public float RearRightWear { get; set; }
    
    public float FrontLeftTemp { get; set; }
    public float FrontRightTemp { get; set; }
    public float RearLeftTemp { get; set; }
    public float RearRightTemp { get; set; }
    
    public float AverageWear => (FrontLeftWear + FrontRightWear + RearLeftWear + RearRightWear) / 4f;
    public float AverageTemp => (FrontLeftTemp + FrontRightTemp + RearLeftTemp + RearRightTemp) / 4f;
}
