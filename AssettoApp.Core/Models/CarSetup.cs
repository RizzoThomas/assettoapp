namespace AssettoApp.Core.Models;

/// <summary>
/// Complete car setup configuration
/// Real parameters that can be modified in AC/ACE
/// </summary>
public class CarSetup
{
    public string CarName { get; set; } = string.Empty;
    public string TrackName { get; set; } = string.Empty;
    public GameType GameType { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Tires
    public TireSetup Tires { get; set; } = new();
    
    // Suspension
    public SuspensionSetup Suspension { get; set; } = new();
    
    // Aerodynamics
    public AerodynamicsSetup Aerodynamics { get; set; } = new();
    
    // Differential
    public DifferentialSetup Differential { get; set; } = new();
    
    // Transmission
    public TransmissionSetup Transmission { get; set; } = new();
    
    // Anti-roll bars
    public AntiRollBarSetup AntiRollBars { get; set; } = new();
    
    // Alignment
    public AlignmentSetup Alignment { get; set; } = new();
    
    // Brake bias
    public BrakeSetup Brakes { get; set; } = new();
}

public class TireSetup
{
    public float FrontLeftPressure { get; set; }   // PSI
    public float FrontRightPressure { get; set; }  // PSI
    public float RearLeftPressure { get; set; }    // PSI
    public float RearRightPressure { get; set; }   // PSI
    public string TireCompound { get; set; } = "Medium";
}

public class SuspensionSetup
{
    // Spring rates (N/mm)
    public float FrontSpringRate { get; set; }
    public float RearSpringRate { get; set; }
    
    // Bump stop rates
    public float FrontBumpStopRate { get; set; }
    public float RearBumpStopRate { get; set; }
    
    // Bump stop range (mm)
    public float FrontBumpStopRange { get; set; }
    public float RearBumpStopRange { get; set; }
    
    // Dampers (clicks or values depending on car)
    public int FrontBumpDamping { get; set; }
    public int FrontReboundDamping { get; set; }
    public int RearBumpDamping { get; set; }
    public int RearReboundDamping { get; set; }
    
    // Ride height (mm)
    public float FrontRideHeight { get; set; }
    public float RearRideHeight { get; set; }
}

public class AerodynamicsSetup
{
    public int FrontWing { get; set; }  // Clicks or angle
    public int RearWing { get; set; }   // Clicks or angle
}

public class DifferentialSetup
{
    public float Preload { get; set; }       // Nm
    public float PowerRamp { get; set; }     // Degrees or percentage
    public float CoastRamp { get; set; }     // Degrees or percentage
}

public class TransmissionSetup
{
    // Gear ratios
    public float[] GearRatios { get; set; } = new float[8];  // Up to 8 gears
    public float FinalDrive { get; set; }
}

public class AntiRollBarSetup
{
    public int Front { get; set; }  // Clicks or N/mm
    public int Rear { get; set; }   // Clicks or N/mm
}

public class AlignmentSetup
{
    // Camber (degrees, negative values)
    public float FrontLeftCamber { get; set; }
    public float FrontRightCamber { get; set; }
    public float RearLeftCamber { get; set; }
    public float RearRightCamber { get; set; }
    
    // Toe (degrees, positive = toe-in)
    public float FrontLeftToe { get; set; }
    public float FrontRightToe { get; set; }
    public float RearLeftToe { get; set; }
    public float RearRightToe { get; set; }
    
    // Caster (degrees)
    public float FrontCaster { get; set; }
}

public class BrakeSetup
{
    public float BrakeBias { get; set; }  // 0.0 (full rear) to 1.0 (full front)
    public float BrakePressure { get; set; }  // Percentage
}
