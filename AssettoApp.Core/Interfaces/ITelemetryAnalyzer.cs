using AssettoApp.Core.Models;

namespace AssettoApp.Core.Interfaces;

/// <summary>
/// Interface for analyzing telemetry data
/// </summary>
public interface ITelemetryAnalyzer
{
    /// <summary>
    /// Analyze a collection of telemetry data points
    /// </summary>
    TelemetryAnalysisResult AnalyzeSession(List<TelemetryData> telemetryData);
    
    /// <summary>
    /// Detect driving style from telemetry
    /// </summary>
    DrivingStyle DetectDrivingStyle(List<TelemetryData> telemetryData);
}

/// <summary>
/// Result of telemetry analysis
/// </summary>
public class TelemetryAnalysisResult
{
    public DrivingStyle DrivingStyle { get; set; }
    public TrackConditionAnalysis TrackConditions { get; set; } = new();
    public TireAnalysis TireAnalysis { get; set; } = new();
    public SuspensionAnalysis SuspensionAnalysis { get; set; } = new();
    public AerodynamicAnalysis AerodynamicAnalysis { get; set; } = new();
    public BalanceAnalysis BalanceAnalysis { get; set; } = new();
}

public enum DrivingStyle
{
    Smooth,
    Aggressive,
    TrailBraker,
    EarlyApex,
    LateApex
}

public class TrackConditionAnalysis
{
    public float AverageTrackTemp { get; set; }
    public float AverageAirTemp { get; set; }
    public float AverageGrip { get; set; }
}

public class TireAnalysis
{
    public float[] AveragePressures { get; set; } = new float[4];
    public float[] AverageTemperatures { get; set; } = new float[4];
    public float[] MaxTemperatures { get; set; } = new float[4];
    public float[] MinTemperatures { get; set; } = new float[4];
    public bool[] OverheatingDetected { get; set; } = new bool[4];
    public bool[] UnderPressureDetected { get; set; } = new bool[4];
    public float[] TemperatureDifferentials { get; set; } = new float[4];  // Inner - Outer temps
}

public class SuspensionAnalysis
{
    public float[] AverageSuspensionTravel { get; set; } = new float[4];
    public float[] MaxSuspensionTravel { get; set; } = new float[4];
    public bool FrontBottoming { get; set; }
    public bool RearBottoming { get; set; }
}

public class AerodynamicAnalysis
{
    public float AverageDownforce { get; set; }
    public float FrontRearDownforceRatio { get; set; }
}

public class BalanceAnalysis
{
    public float OversteerTendency { get; set; }  // -1 (oversteer) to +1 (understeer)
    public int OversteerCornerCount { get; set; }
    public int UndersteerCornerCount { get; set; }
    public float AverageSlipAngleFront { get; set; }
    public float AverageSlipAngleRear { get; set; }
}
