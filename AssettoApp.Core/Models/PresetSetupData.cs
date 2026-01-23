namespace AssettoApp.Core.Models;

/// <summary>
/// Preset setup data for offline mode generation
/// Based on known physics models and best practices
/// </summary>
public class PresetSetupData
{
    /// <summary>
    /// Car characteristics preset
    /// </summary>
    public class CarPreset
    {
        public string CarName { get; set; } = string.Empty;
        public string CarClass { get; set; } = string.Empty; // GT3, GT4, Formula, etc.
        public int Power { get; set; } // HP
        public int Weight { get; set; } // kg
        public string DriveType { get; set; } = "RWD"; // RWD, FWD, AWD
        
        // Default setup values based on car class
        public float DefaultFrontSpring { get; set; } = 80.0f; // N/mm
        public float DefaultRearSpring { get; set; } = 85.0f;
        public float DefaultFrontCamber { get; set; } = -2.8f; // degrees
        public float DefaultRearCamber { get; set; } = -2.5f;
        public int DefaultFrontWing { get; set; } = 3; // clicks
        public int DefaultRearWing { get; set; } = 5;
    }

    /// <summary>
    /// Track characteristics preset
    /// </summary>
    public class TrackPreset
    {
        public string TrackName { get; set; } = string.Empty;
        public float LengthKm { get; set; }
        public string TrackType { get; set; } = "Road"; // Road, Street, Oval
        public int FastCornerCount { get; set; }
        public int SlowCornerCount { get; set; }
        public float LongestStraightKm { get; set; }
        
        // Track-specific setup adjustments
        public string AeroLevel { get; set; } = "Medium"; // Low, Medium, High
        public string SuspensionStiffness { get; set; } = "Medium"; // Soft, Medium, Stiff
    }
}
