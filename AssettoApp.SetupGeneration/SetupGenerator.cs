using AssettoApp.Core.Interfaces;
using AssettoApp.Core.Models;
using AssettoApp.Core.Repositories;
using System.Text;

namespace AssettoApp.SetupGeneration;

/// <summary>
/// Generates optimized car setups based on telemetry analysis or preset data
/// Uses data-driven approach to adjust setup parameters
/// </summary>
public class SetupGenerator : ISetupGenerator
{
    private readonly PresetDataRepository _presetRepository;

    public SetupGenerator()
    {
        _presetRepository = new PresetDataRepository();
    }

    public CarSetup GenerateSetup(
        TelemetryAnalysisResult analysisResult,
        string carName,
        string trackName,
        GameType gameType)
    {
        var setup = new CarSetup
        {
            CarName = carName,
            TrackName = trackName,
            GameType = gameType,
            CreatedAt = DateTime.Now
        };

        // Optimize tire pressures
        OptimizeTirePressures(setup, analysisResult.TireAnalysis);

        // Optimize suspension
        OptimizeSuspension(setup, analysisResult.SuspensionAnalysis, analysisResult.DrivingStyle);

        // Optimize aerodynamics
        OptimizeAerodynamics(setup, analysisResult.AerodynamicAnalysis, analysisResult.BalanceAnalysis);

        // Optimize differential
        OptimizeDifferential(setup, analysisResult.BalanceAnalysis, analysisResult.DrivingStyle);

        // Optimize anti-roll bars
        OptimizeAntiRollBars(setup, analysisResult.BalanceAnalysis);

        // Optimize alignment (camber and toe)
        OptimizeAlignment(setup, analysisResult.TireAnalysis);

        // Optimize brake balance
        OptimizeBrakes(setup, analysisResult.BalanceAnalysis);

        return setup;
    }

    /// <summary>
    /// Generate setup for offline mode using preset data
    /// No telemetry required - uses car and track characteristics
    /// </summary>
    public CarSetup GenerateOfflineSetup(
        string carName,
        string trackName,
        GameType gameType,
        string drivingStylePreference = "Balanced")
    {
        var setup = new CarSetup
        {
            CarName = carName,
            TrackName = trackName,
            GameType = gameType,
            CreatedAt = DateTime.Now
        };

        // Get preset data
        var carPreset = _presetRepository.GetCarPreset(carName);
        var trackPreset = _presetRepository.GetTrackPreset(trackName);

        if (carPreset == null || trackPreset == null)
        {
            // Fallback to generic setup if presets not found
            return GenerateFallbackSetup(setup);
        }

        // Apply car-specific defaults
        setup.Suspension.FrontSpringRate = carPreset.DefaultFrontSpring;
        setup.Suspension.RearSpringRate = carPreset.DefaultRearSpring;
        setup.Alignment.FrontLeftCamber = carPreset.DefaultFrontCamber;
        setup.Alignment.FrontRightCamber = carPreset.DefaultFrontCamber;
        setup.Alignment.RearLeftCamber = carPreset.DefaultRearCamber;
        setup.Alignment.RearRightCamber = carPreset.DefaultRearCamber;
        setup.Aerodynamics.FrontWing = carPreset.DefaultFrontWing;
        setup.Aerodynamics.RearWing = carPreset.DefaultRearWing;

        // Adjust for track characteristics
        ApplyTrackCharacteristics(setup, trackPreset, carPreset);

        // Apply driving style preference
        ApplyDrivingStylePreference(setup, drivingStylePreference);

        // Set remaining parameters to safe defaults
        SetDefaultParameters(setup);

        return setup;
    }

    private void ApplyTrackCharacteristics(
        CarSetup setup,
        PresetSetupData.TrackPreset trackPreset,
        PresetSetupData.CarPreset carPreset)
    {
        // Aero adjustments based on track type
        switch (trackPreset.AeroLevel)
        {
            case "Low": // High-speed tracks like Monza
                setup.Aerodynamics.FrontWing = Math.Max(0, carPreset.DefaultFrontWing - 1);
                setup.Aerodynamics.RearWing = Math.Max(0, carPreset.DefaultRearWing - 2);
                setup.Suspension.FrontRideHeight = 58.0f; // Higher for less drag
                setup.Suspension.RearRideHeight = 63.0f;
                break;

            case "High": // Downforce tracks like Mugello, Silverstone
                setup.Aerodynamics.FrontWing = Math.Min(10, carPreset.DefaultFrontWing + 1);
                setup.Aerodynamics.RearWing = Math.Min(12, carPreset.DefaultRearWing + 1);
                setup.Suspension.FrontRideHeight = 52.0f; // Lower for more downforce
                setup.Suspension.RearRideHeight = 57.0f;
                break;

            default: // "Medium" - balanced
                setup.Suspension.FrontRideHeight = 55.0f;
                setup.Suspension.RearRideHeight = 60.0f;
                break;
        }

        // Suspension stiffness adjustments
        switch (trackPreset.SuspensionStiffness)
        {
            case "Soft": // Bumpy or kerb-heavy tracks
                setup.Suspension.FrontSpringRate -= 5.0f;
                setup.Suspension.RearSpringRate -= 5.0f;
                setup.Suspension.FrontBumpDamping = 5;
                setup.Suspension.FrontReboundDamping = 6;
                setup.Suspension.RearBumpDamping = 5;
                setup.Suspension.RearReboundDamping = 6;
                break;

            case "Stiff": // Smooth tracks with elevation changes
                setup.Suspension.FrontSpringRate += 5.0f;
                setup.Suspension.RearSpringRate += 5.0f;
                setup.Suspension.FrontBumpDamping = 7;
                setup.Suspension.FrontReboundDamping = 8;
                setup.Suspension.RearBumpDamping = 7;
                setup.Suspension.RearReboundDamping = 8;
                break;

            default: // "Medium"
                setup.Suspension.FrontBumpDamping = 6;
                setup.Suspension.FrontReboundDamping = 7;
                setup.Suspension.RearBumpDamping = 6;
                setup.Suspension.RearReboundDamping = 7;
                break;
        }

        // Gear ratios - optimize final drive for longest straight
        if (trackPreset.LongestStraightKm > 1.5f) // Long straights
        {
            // Longer gearing for top speed
            setup.Transmission.FinalDrive = 2.8f;
        }
        else if (trackPreset.LongestStraightKm < 0.7f) // Short straights
        {
            // Shorter gearing for acceleration
            setup.Transmission.FinalDrive = 3.5f;
        }
        else
        {
            setup.Transmission.FinalDrive = 3.2f; // Balanced
        }
    }

    private void ApplyDrivingStylePreference(CarSetup setup, string stylePreference)
    {
        switch (stylePreference.ToLower())
        {
            case "aggressive":
                // Stiffer setup for aggressive inputs
                setup.Suspension.FrontSpringRate += 5.0f;
                setup.Suspension.RearSpringRate += 5.0f;
                setup.Differential.PowerRamp += 5.0f;
                setup.AntiRollBars.Front = Math.Min(7, setup.AntiRollBars.Front + 1);
                setup.AntiRollBars.Rear = Math.Min(7, setup.AntiRollBars.Rear + 1);
                break;

            case "smooth":
                // Softer setup for smooth driving
                setup.Suspension.FrontSpringRate -= 5.0f;
                setup.Suspension.RearSpringRate -= 5.0f;
                setup.Differential.PowerRamp -= 5.0f;
                setup.AntiRollBars.Front = Math.Max(1, setup.AntiRollBars.Front - 1);
                setup.AntiRollBars.Rear = Math.Max(1, setup.AntiRollBars.Rear - 1);
                break;

            case "oversteery":
                // Setup promoting rotation
                setup.Differential.PowerRamp += 10.0f;
                setup.AntiRollBars.Front = Math.Max(1, setup.AntiRollBars.Front - 1);
                setup.AntiRollBars.Rear = Math.Min(7, setup.AntiRollBars.Rear + 1);
                setup.Aerodynamics.RearWing = Math.Max(0, setup.Aerodynamics.RearWing - 1);
                setup.Brakes.BrakeBias += 0.02f;
                break;

            case "understeery":
                // Stable setup with less rotation
                setup.Differential.PowerRamp -= 10.0f;
                setup.AntiRollBars.Front = Math.Min(7, setup.AntiRollBars.Front + 1);
                setup.AntiRollBars.Rear = Math.Max(1, setup.AntiRollBars.Rear - 1);
                setup.Aerodynamics.FrontWing = Math.Min(10, setup.Aerodynamics.FrontWing + 1);
                setup.Brakes.BrakeBias -= 0.02f;
                break;

            default: // "balanced"
                // Keep defaults
                break;
        }
    }

    private void SetDefaultParameters(CarSetup setup)
    {
        // Tire pressures - safe starting point
        if (setup.Tires.FrontLeftPressure == 0)
        {
            setup.Tires.FrontLeftPressure = 26.0f;
            setup.Tires.FrontRightPressure = 26.0f;
            setup.Tires.RearLeftPressure = 26.0f;
            setup.Tires.RearRightPressure = 26.0f;
        }

        // Differential defaults if not set
        if (setup.Differential.Preload == 0)
        {
            setup.Differential.Preload = 50.0f;
            setup.Differential.PowerRamp = 60.0f;
            setup.Differential.CoastRamp = 40.0f;
        }

        // Anti-roll bars defaults
        if (setup.AntiRollBars.Front == 0)
        {
            setup.AntiRollBars.Front = 3;
            setup.AntiRollBars.Rear = 3;
        }

        // Alignment defaults
        if (setup.Alignment.FrontLeftToe == 0)
        {
            setup.Alignment.FrontLeftToe = 0.05f;
            setup.Alignment.FrontRightToe = 0.05f;
            setup.Alignment.RearLeftToe = 0.1f;
            setup.Alignment.RearRightToe = 0.1f;
            setup.Alignment.FrontCaster = 11.0f;
        }

        // Brakes defaults
        if (setup.Brakes.BrakeBias == 0)
        {
            setup.Brakes.BrakeBias = 0.56f;
            setup.Brakes.BrakePressure = 100.0f;
        }

        // Ensure bounds
        setup.Brakes.BrakeBias = Math.Clamp(setup.Brakes.BrakeBias, 0.5f, 0.65f);
    }

    private CarSetup GenerateFallbackSetup(CarSetup setup)
    {
        // Generic GT3-style setup when no presets available
        setup.Tires.FrontLeftPressure = 26.0f;
        setup.Tires.FrontRightPressure = 26.0f;
        setup.Tires.RearLeftPressure = 26.0f;
        setup.Tires.RearRightPressure = 26.0f;

        setup.Suspension.FrontSpringRate = 80.0f;
        setup.Suspension.RearSpringRate = 85.0f;
        setup.Suspension.FrontBumpDamping = 6;
        setup.Suspension.FrontReboundDamping = 7;
        setup.Suspension.RearBumpDamping = 6;
        setup.Suspension.RearReboundDamping = 7;
        setup.Suspension.FrontRideHeight = 55.0f;
        setup.Suspension.RearRideHeight = 60.0f;

        setup.Aerodynamics.FrontWing = 3;
        setup.Aerodynamics.RearWing = 5;

        setup.Differential.Preload = 50.0f;
        setup.Differential.PowerRamp = 60.0f;
        setup.Differential.CoastRamp = 40.0f;

        setup.AntiRollBars.Front = 3;
        setup.AntiRollBars.Rear = 3;

        setup.Alignment.FrontLeftCamber = -2.8f;
        setup.Alignment.FrontRightCamber = -2.8f;
        setup.Alignment.RearLeftCamber = -2.5f;
        setup.Alignment.RearRightCamber = -2.5f;
        setup.Alignment.FrontLeftToe = 0.05f;
        setup.Alignment.FrontRightToe = 0.05f;
        setup.Alignment.RearLeftToe = 0.1f;
        setup.Alignment.RearRightToe = 0.1f;
        setup.Alignment.FrontCaster = 11.0f;

        setup.Brakes.BrakeBias = 0.56f;
        setup.Brakes.BrakePressure = 100.0f;

        setup.Transmission.FinalDrive = 3.2f;

        return setup;
    }

    private void OptimizeTirePressures(CarSetup setup, TireAnalysis tireAnalysis)
    {
        // Base pressures (PSI) - typical starting points
        float basePressure = 26.0f;

        for (int i = 0; i < 4; i++)
        {
            float adjustedPressure = basePressure;

            // Adjust for overheating
            if (tireAnalysis.OverheatingDetected[i])
            {
                adjustedPressure -= 1.0f; // Lower pressure to reduce heat
            }

            // Adjust for temperature
            if (tireAnalysis.AverageTemperatures[i] < 75f)
            {
                adjustedPressure += 1.0f; // Increase to bring temps up
            }
            else if (tireAnalysis.AverageTemperatures[i] > 95f)
            {
                adjustedPressure -= 1.0f; // Decrease to cool down
            }

            // Apply adjustments to setup
            switch (i)
            {
                case 0: setup.Tires.FrontLeftPressure = adjustedPressure; break;
                case 1: setup.Tires.FrontRightPressure = adjustedPressure; break;
                case 2: setup.Tires.RearLeftPressure = adjustedPressure; break;
                case 3: setup.Tires.RearRightPressure = adjustedPressure; break;
            }
        }
    }

    private void OptimizeSuspension(CarSetup setup, SuspensionAnalysis suspAnalysis, DrivingStyle drivingStyle)
    {
        // Base spring rates (N/mm) - typical GT3 values
        float baseFrontSpring = 80.0f;
        float baseRearSpring = 85.0f;

        // Adjust for bottoming
        if (suspAnalysis.FrontBottoming)
        {
            baseFrontSpring += 10.0f; // Stiffer springs
            setup.Suspension.FrontBumpStopRate += 5.0f;
        }

        if (suspAnalysis.RearBottoming)
        {
            baseRearSpring += 10.0f;
            setup.Suspension.RearBumpStopRate += 5.0f;
        }

        // Adjust for driving style
        if (drivingStyle == DrivingStyle.Aggressive)
        {
            baseFrontSpring += 5.0f;
            baseRearSpring += 5.0f;
        }
        else if (drivingStyle == DrivingStyle.Smooth)
        {
            baseFrontSpring -= 5.0f;
            baseRearSpring -= 5.0f;
        }

        setup.Suspension.FrontSpringRate = baseFrontSpring;
        setup.Suspension.RearSpringRate = baseRearSpring;

        // Dampers - balanced starting point
        setup.Suspension.FrontBumpDamping = 6;
        setup.Suspension.FrontReboundDamping = 7;
        setup.Suspension.RearBumpDamping = 6;
        setup.Suspension.RearReboundDamping = 7;

        // Ride height (mm) - lower for aero, higher for kerb riding
        setup.Suspension.FrontRideHeight = 55.0f;
        setup.Suspension.RearRideHeight = 60.0f;
    }

    private void OptimizeAerodynamics(CarSetup setup, AerodynamicAnalysis aeroAnalysis, BalanceAnalysis balanceAnalysis)
    {
        // Base wing settings (clicks or degrees depending on car)
        int baseFrontWing = 3;
        int baseRearWing = 5;

        // Adjust for balance
        if (balanceAnalysis.OversteerTendency < -0.3f) // Oversteer
        {
            baseRearWing += 1; // More rear downforce
        }
        else if (balanceAnalysis.OversteerTendency > 0.3f) // Understeer
        {
            baseFrontWing += 1; // More front downforce
        }

        setup.Aerodynamics.FrontWing = baseFrontWing;
        setup.Aerodynamics.RearWing = baseRearWing;
    }

    private void OptimizeDifferential(CarSetup setup, BalanceAnalysis balanceAnalysis, DrivingStyle drivingStyle)
    {
        // Base differential settings
        float basePreload = 50.0f; // Nm
        float basePowerRamp = 60.0f; // Degrees
        float baseCoastRamp = 40.0f; // Degrees

        // Adjust for oversteer/understeer on power
        if (balanceAnalysis.OversteerTendency < -0.3f)
        {
            basePowerRamp -= 10.0f; // Less locking on power = less oversteer
        }
        else if (balanceAnalysis.OversteerTendency > 0.3f)
        {
            basePowerRamp += 10.0f; // More locking = better rotation
        }

        // Adjust for driving style
        if (drivingStyle == DrivingStyle.TrailBraker)
        {
            baseCoastRamp -= 10.0f; // Less coast locking for better rotation
        }

        setup.Differential.Preload = basePreload;
        setup.Differential.PowerRamp = basePowerRamp;
        setup.Differential.CoastRamp = baseCoastRamp;
    }

    private void OptimizeAntiRollBars(CarSetup setup, BalanceAnalysis balanceAnalysis)
    {
        // Base ARB settings (clicks)
        int baseFrontARB = 3;
        int baseRearARB = 3;

        // Adjust for balance
        if (balanceAnalysis.OversteerTendency < -0.3f) // Oversteer
        {
            baseFrontARB += 1; // Stiffer front = more understeer
            baseRearARB -= 1; // Softer rear = less oversteer
        }
        else if (balanceAnalysis.OversteerTendency > 0.3f) // Understeer
        {
            baseFrontARB -= 1; // Softer front = less understeer
            baseRearARB += 1; // Stiffer rear = more rotation
        }

        setup.AntiRollBars.Front = Math.Max(1, baseFrontARB);
        setup.AntiRollBars.Rear = Math.Max(1, baseRearARB);
    }

    private void OptimizeAlignment(CarSetup setup, TireAnalysis tireAnalysis)
    {
        // Base camber settings (degrees, negative)
        float baseFrontCamber = -2.8f;
        float baseRearCamber = -2.5f;

        // Adjust based on tire temperature differentials
        for (int i = 0; i < 4; i++)
        {
            float tempDiff = tireAnalysis.TemperatureDifferentials[i];
            
            // If inner is much hotter than outer, need more negative camber
            float camberAdjust = tempDiff > 5f ? -0.3f : (tempDiff < -5f ? 0.3f : 0f);

            if (i < 2) // Front tires
            {
                if (i == 0) setup.Alignment.FrontLeftCamber = baseFrontCamber + camberAdjust;
                else setup.Alignment.FrontRightCamber = baseFrontCamber + camberAdjust;
            }
            else // Rear tires
            {
                if (i == 2) setup.Alignment.RearLeftCamber = baseRearCamber + camberAdjust;
                else setup.Alignment.RearRightCamber = baseRearCamber + camberAdjust;
            }
        }

        // Toe settings (degrees)
        setup.Alignment.FrontLeftToe = 0.05f;  // Slight toe-in for stability
        setup.Alignment.FrontRightToe = 0.05f;
        setup.Alignment.RearLeftToe = 0.1f;    // More toe-in at rear
        setup.Alignment.RearRightToe = 0.1f;

        // Caster (degrees)
        setup.Alignment.FrontCaster = 11.0f;  // Higher caster for better feel
    }

    private void OptimizeBrakes(CarSetup setup, BalanceAnalysis balanceAnalysis)
    {
        // Base brake bias (0.5 = 50% front)
        float baseBrakeBias = 0.56f; // Typical GT3 starting point

        // Adjust slightly for balance issues
        if (balanceAnalysis.OversteerTendency < -0.3f)
        {
            baseBrakeBias += 0.02f; // More front bias to reduce rotation
        }

        setup.Brakes.BrakeBias = Math.Clamp(baseBrakeBias, 0.5f, 0.65f);
        setup.Brakes.BrakePressure = 100.0f; // Percentage
    }

    public async Task<bool> ExportSetupAsync(CarSetup setup, string outputPath)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine($"; Assetto Corsa Setup File");
            sb.AppendLine($"; Generated by AssettoApp on {setup.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"; Car: {setup.CarName}");
            sb.AppendLine($"; Track: {setup.TrackName}");
            sb.AppendLine();

            // Tires section
            sb.AppendLine("[TYRES]");
            sb.AppendLine($"PRESSURE_LF={setup.Tires.FrontLeftPressure:F1}");
            sb.AppendLine($"PRESSURE_RF={setup.Tires.FrontRightPressure:F1}");
            sb.AppendLine($"PRESSURE_LR={setup.Tires.RearLeftPressure:F1}");
            sb.AppendLine($"PRESSURE_RR={setup.Tires.RearRightPressure:F1}");
            sb.AppendLine();

            // Alignment section
            sb.AppendLine("[ALIGNMENT]");
            sb.AppendLine($"CAMBER_LF={setup.Alignment.FrontLeftCamber:F2}");
            sb.AppendLine($"CAMBER_RF={setup.Alignment.FrontRightCamber:F2}");
            sb.AppendLine($"CAMBER_LR={setup.Alignment.RearLeftCamber:F2}");
            sb.AppendLine($"CAMBER_RR={setup.Alignment.RearRightCamber:F2}");
            sb.AppendLine($"TOE_LF={setup.Alignment.FrontLeftToe:F3}");
            sb.AppendLine($"TOE_RF={setup.Alignment.FrontRightToe:F3}");
            sb.AppendLine($"TOE_LR={setup.Alignment.RearLeftToe:F3}");
            sb.AppendLine($"TOE_RR={setup.Alignment.RearRightToe:F3}");
            sb.AppendLine($"CASTER_LF={setup.Alignment.FrontCaster:F1}");
            sb.AppendLine($"CASTER_RF={setup.Alignment.FrontCaster:F1}");
            sb.AppendLine();

            // Suspension section
            sb.AppendLine("[SUSPENSION]");
            sb.AppendLine($"SPRING_RATE_F={setup.Suspension.FrontSpringRate:F1}");
            sb.AppendLine($"SPRING_RATE_R={setup.Suspension.RearSpringRate:F1}");
            sb.AppendLine($"RIDE_HEIGHT_F={setup.Suspension.FrontRideHeight:F0}");
            sb.AppendLine($"RIDE_HEIGHT_R={setup.Suspension.RearRideHeight:F0}");
            sb.AppendLine($"BUMP_F={setup.Suspension.FrontBumpDamping}");
            sb.AppendLine($"REBOUND_F={setup.Suspension.FrontReboundDamping}");
            sb.AppendLine($"BUMP_R={setup.Suspension.RearBumpDamping}");
            sb.AppendLine($"REBOUND_R={setup.Suspension.RearReboundDamping}");
            sb.AppendLine();

            // Aerodynamics section
            sb.AppendLine("[AERO]");
            sb.AppendLine($"FRONT_WING={setup.Aerodynamics.FrontWing}");
            sb.AppendLine($"REAR_WING={setup.Aerodynamics.RearWing}");
            sb.AppendLine();

            // Anti-roll bars
            sb.AppendLine("[ARB]");
            sb.AppendLine($"FRONT={setup.AntiRollBars.Front}");
            sb.AppendLine($"REAR={setup.AntiRollBars.Rear}");
            sb.AppendLine();

            // Differential
            sb.AppendLine("[DIFF]");
            sb.AppendLine($"PRELOAD={setup.Differential.Preload:F1}");
            sb.AppendLine($"POWER={setup.Differential.PowerRamp:F1}");
            sb.AppendLine($"COAST={setup.Differential.CoastRamp:F1}");
            sb.AppendLine();

            // Brakes
            sb.AppendLine("[BRAKES]");
            sb.AppendLine($"BIAS={setup.Brakes.BrakeBias:F3}");
            sb.AppendLine($"PRESSURE={setup.Brakes.BrakePressure:F1}");

            await File.WriteAllTextAsync(outputPath, sb.ToString());
            return true;
        }
        catch
        {
            return false;
        }
    }
}
