using AssettoApp.Core.Interfaces;
using AssettoApp.Core.Models;

namespace AssettoApp.TelemetryAnalysis;

/// <summary>
/// Analyzes telemetry data to provide insights for setup optimization
/// </summary>
public class TelemetryAnalyzer : ITelemetryAnalyzer
{
    public TelemetryAnalysisResult AnalyzeSession(List<TelemetryData> telemetryData)
    {
        if (telemetryData == null || telemetryData.Count == 0)
        {
            return new TelemetryAnalysisResult();
        }

        var result = new TelemetryAnalysisResult
        {
            DrivingStyle = DetectDrivingStyle(telemetryData),
            TrackConditions = AnalyzeTrackConditions(telemetryData),
            TireAnalysis = AnalyzeTires(telemetryData),
            SuspensionAnalysis = AnalyzeSuspension(telemetryData),
            AerodynamicAnalysis = AnalyzeAerodynamics(telemetryData),
            BalanceAnalysis = AnalyzeBalance(telemetryData)
        };

        return result;
    }

    public DrivingStyle DetectDrivingStyle(List<TelemetryData> telemetryData)
    {
        if (telemetryData == null || telemetryData.Count == 0)
            return DrivingStyle.Smooth;

        // Analyze braking patterns
        float avgBrakeForce = telemetryData.Average(t => t.Brake);
        float maxBrakeForce = telemetryData.Max(t => t.Brake);
        
        // Analyze throttle application
        float avgThrottleChange = CalculateAverageThrottleChange(telemetryData);
        
        // Analyze steering inputs
        float avgSteeringChange = CalculateAverageSteeringChange(telemetryData);

        // Classify driving style based on input aggression
        if (avgSteeringChange > 0.5f || avgThrottleChange > 0.4f)
            return DrivingStyle.Aggressive;
        
        if (avgBrakeForce > 0.7f && HasLateApexPattern(telemetryData))
            return DrivingStyle.TrailBraker;
        
        if (HasEarlyApexPattern(telemetryData))
            return DrivingStyle.EarlyApex;
        
        if (HasLateApexPattern(telemetryData))
            return DrivingStyle.LateApex;

        return DrivingStyle.Smooth;
    }

    private TrackConditionAnalysis AnalyzeTrackConditions(List<TelemetryData> telemetryData)
    {
        return new TrackConditionAnalysis
        {
            AverageTrackTemp = telemetryData.Average(t => t.TrackTemperature),
            AverageAirTemp = telemetryData.Average(t => t.AirTemperature),
            AverageGrip = telemetryData.Average(t => t.TrackGrip)
        };
    }

    private TireAnalysis AnalyzeTires(List<TelemetryData> telemetryData)
    {
        var analysis = new TireAnalysis();
        
        for (int i = 0; i < 4; i++)
        {
            var tireData = telemetryData.Select(t => t.Tires[i]).ToList();
            
            analysis.AveragePressures[i] = tireData.Average(t => t.Pressure);
            analysis.AverageTemperatures[i] = tireData.Average(t => t.Temperature);
            analysis.MaxTemperatures[i] = tireData.Max(t => t.Temperature);
            analysis.MinTemperatures[i] = tireData.Min(t => t.Temperature);
            
            // Detect overheating (typically > 100°C for most compounds)
            analysis.OverheatingDetected[i] = analysis.MaxTemperatures[i] > 100f;
            
            // Detect low pressure (typically < 20 PSI)
            analysis.UnderPressureDetected[i] = analysis.AveragePressures[i] < 20f;
            
            // Temperature differential (inner vs outer)
            analysis.TemperatureDifferentials[i] = tireData.Average(t => t.InnerTemperature - t.OuterTemperature);
        }
        
        return analysis;
    }

    private SuspensionAnalysis AnalyzeSuspension(List<TelemetryData> telemetryData)
    {
        var analysis = new SuspensionAnalysis();
        
        for (int i = 0; i < 4; i++)
        {
            var suspensionData = telemetryData.Select(t => t.SuspensionTravel[i]).ToList();
            analysis.AverageSuspensionTravel[i] = suspensionData.Average();
            analysis.MaxSuspensionTravel[i] = suspensionData.Max();
        }
        
        // Detect bottoming (suspension travel > 90% of maximum)
        // Typical max travel is around 50-80mm
        analysis.FrontBottoming = analysis.MaxSuspensionTravel[0] > 70 || analysis.MaxSuspensionTravel[1] > 70;
        analysis.RearBottoming = analysis.MaxSuspensionTravel[2] > 70 || analysis.MaxSuspensionTravel[3] > 70;
        
        return analysis;
    }

    private AerodynamicAnalysis AnalyzeAerodynamics(List<TelemetryData> telemetryData)
    {
        var analysis = new AerodynamicAnalysis
        {
            AverageDownforce = telemetryData.Average(t => t.AerodynamicLoad)
        };
        
        // Calculate front/rear balance if data available
        var frontDownforceAvg = telemetryData.Average(t => t.FrontDownforce);
        var rearDownforceAvg = telemetryData.Average(t => t.RearDownforce);
        
        if (rearDownforceAvg > 0)
        {
            analysis.FrontRearDownforceRatio = frontDownforceAvg / rearDownforceAvg;
        }
        
        return analysis;
    }

    private BalanceAnalysis AnalyzeBalance(List<TelemetryData> telemetryData)
    {
        var analysis = new BalanceAnalysis();
        
        // Analyze understeer/oversteer tendency
        var steeringRatios = telemetryData.Select(t => t.SteeringRatio).ToList();
        analysis.OversteerTendency = steeringRatios.Average();
        
        // Count occurrences
        analysis.OversteerCornerCount = steeringRatios.Count(sr => sr < -0.5f);
        analysis.UndersteerCornerCount = steeringRatios.Count(sr => sr > 0.5f);
        
        // Average slip angles
        for (int i = 0; i < telemetryData.Count; i++)
        {
            var data = telemetryData[i];
            analysis.AverageSlipAngleFront += (Math.Abs(data.SlipAngles[0]) + Math.Abs(data.SlipAngles[1])) / 2f;
            analysis.AverageSlipAngleRear += (Math.Abs(data.SlipAngles[2]) + Math.Abs(data.SlipAngles[3])) / 2f;
        }
        
        analysis.AverageSlipAngleFront /= telemetryData.Count;
        analysis.AverageSlipAngleRear /= telemetryData.Count;
        
        return analysis;
    }

    private float CalculateAverageThrottleChange(List<TelemetryData> telemetryData)
    {
        float totalChange = 0;
        for (int i = 1; i < telemetryData.Count; i++)
        {
            totalChange += Math.Abs(telemetryData[i].Throttle - telemetryData[i - 1].Throttle);
        }
        return telemetryData.Count > 1 ? totalChange / (telemetryData.Count - 1) : 0;
    }

    private float CalculateAverageSteeringChange(List<TelemetryData> telemetryData)
    {
        float totalChange = 0;
        for (int i = 1; i < telemetryData.Count; i++)
        {
            totalChange += Math.Abs(telemetryData[i].SteeringAngle - telemetryData[i - 1].SteeringAngle);
        }
        return telemetryData.Count > 1 ? totalChange / (telemetryData.Count - 1) : 0;
    }

    private bool HasEarlyApexPattern(List<TelemetryData> telemetryData)
    {
        // Simplified: early apex means throttle application while still turning
        int earlyThrottleCount = 0;
        foreach (var data in telemetryData)
        {
            if (Math.Abs(data.SteeringAngle) > 10 && data.Throttle > 0.5f)
                earlyThrottleCount++;
        }
        return (float)earlyThrottleCount / telemetryData.Count > 0.2f;
    }

    private bool HasLateApexPattern(List<TelemetryData> telemetryData)
    {
        // Simplified: late apex means braking deeper into corner
        int lateBrakeCount = 0;
        foreach (var data in telemetryData)
        {
            if (Math.Abs(data.SteeringAngle) > 5 && data.Brake > 0.3f)
                lateBrakeCount++;
        }
        return (float)lateBrakeCount / telemetryData.Count > 0.15f;
    }
}
