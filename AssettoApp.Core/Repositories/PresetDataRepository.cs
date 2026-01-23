using AssettoApp.Core.Models;
using static AssettoApp.Core.Models.PresetSetupData;

namespace AssettoApp.Core.Repositories;

/// <summary>
/// Repository for preset car and track data used in offline mode
/// Data based on real vehicle characteristics and track layouts
/// </summary>
public class PresetDataRepository
{
    private readonly Dictionary<string, CarPreset> _carPresets;
    private readonly Dictionary<string, TrackPreset> _trackPresets;

    public PresetDataRepository()
    {
        _carPresets = InitializeCarPresets();
        _trackPresets = InitializeTrackPresets();
    }

    /// <summary>
    /// Get all available car names
    /// </summary>
    public IEnumerable<string> GetAvailableCars() => _carPresets.Keys.OrderBy(x => x);

    /// <summary>
    /// Get all available track names
    /// </summary>
    public IEnumerable<string> GetAvailableTracks() => _trackPresets.Keys.OrderBy(x => x);

    /// <summary>
    /// Get car preset data
    /// </summary>
    public CarPreset? GetCarPreset(string carName)
    {
        return _carPresets.TryGetValue(carName, out var preset) ? preset : null;
    }

    /// <summary>
    /// Get track preset data
    /// </summary>
    public TrackPreset? GetTrackPreset(string trackName)
    {
        return _trackPresets.TryGetValue(trackName, out var preset) ? preset : null;
    }

    private Dictionary<string, CarPreset> InitializeCarPresets()
    {
        // Real car data based on known specifications
        return new Dictionary<string, CarPreset>
        {
            // GT3 Cars - Data from manufacturer specs and racing regulations
            ["Ferrari 488 GT3"] = new CarPreset
            {
                CarName = "Ferrari 488 GT3",
                CarClass = "GT3",
                Power = 550,
                Weight = 1245,
                DriveType = "RWD",
                DefaultFrontSpring = 85.0f,
                DefaultRearSpring = 90.0f,
                DefaultFrontCamber = -3.0f,
                DefaultRearCamber = -2.5f,
                DefaultFrontWing = 3,
                DefaultRearWing = 6
            },
            ["Mercedes AMG GT3"] = new CarPreset
            {
                CarName = "Mercedes AMG GT3",
                CarClass = "GT3",
                Power = 558,
                Weight = 1285,
                DriveType = "RWD",
                DefaultFrontSpring = 82.0f,
                DefaultRearSpring = 88.0f,
                DefaultFrontCamber = -2.9f,
                DefaultRearCamber = -2.4f,
                DefaultFrontWing = 3,
                DefaultRearWing = 6
            },
            ["Porsche 911 GT3 R"] = new CarPreset
            {
                CarName = "Porsche 911 GT3 R",
                CarClass = "GT3",
                Power = 550,
                Weight = 1245,
                DriveType = "RWD",
                DefaultFrontSpring = 80.0f,
                DefaultRearSpring = 95.0f, // Rear engine layout requires stiffer rear
                DefaultFrontCamber = -2.8f,
                DefaultRearCamber = -2.8f,
                DefaultFrontWing = 3,
                DefaultRearWing = 7
            },
            ["BMW M6 GT3"] = new CarPreset
            {
                CarName = "BMW M6 GT3",
                CarClass = "GT3",
                Power = 585,
                Weight = 1300,
                DriveType = "RWD",
                DefaultFrontSpring = 84.0f,
                DefaultRearSpring = 89.0f,
                DefaultFrontCamber = -3.1f,
                DefaultRearCamber = -2.6f,
                DefaultFrontWing = 3,
                DefaultRearWing = 6
            },
            
            // GT4 Cars - Lower power, street-based
            ["Porsche Cayman GT4"] = new CarPreset
            {
                CarName = "Porsche Cayman GT4",
                CarClass = "GT4",
                Power = 385,
                Weight = 1350,
                DriveType = "RWD",
                DefaultFrontSpring = 70.0f,
                DefaultRearSpring = 75.0f,
                DefaultFrontCamber = -2.5f,
                DefaultRearCamber = -2.0f,
                DefaultFrontWing = 0,
                DefaultRearWing = 0
            },
            ["BMW M4 GT4"] = new CarPreset
            {
                CarName = "BMW M4 GT4",
                CarClass = "GT4",
                Power = 431,
                Weight = 1415,
                DriveType = "RWD",
                DefaultFrontSpring = 72.0f,
                DefaultRearSpring = 76.0f,
                DefaultFrontCamber = -2.6f,
                DefaultRearCamber = -2.1f,
                DefaultFrontWing = 0,
                DefaultRearWing = 0
            },
            
            // Formula Cars - High downforce, lightweight
            ["Formula RSS 2"] = new CarPreset
            {
                CarName = "Formula RSS 2",
                CarClass = "Formula",
                Power = 740,
                Weight = 795,
                DriveType = "RWD",
                DefaultFrontSpring = 120.0f,
                DefaultRearSpring = 140.0f,
                DefaultFrontCamber = -3.5f,
                DefaultRearCamber = -2.0f,
                DefaultFrontWing = 8,
                DefaultRearWing = 10
            },
            
            // Road Cars - Street-legal performance
            ["Lamborghini Huracan Performante"] = new CarPreset
            {
                CarName = "Lamborghini Huracan Performante",
                CarClass = "Road",
                Power = 640,
                Weight = 1382,
                DriveType = "AWD",
                DefaultFrontSpring = 60.0f,
                DefaultRearSpring = 65.0f,
                DefaultFrontCamber = -2.0f,
                DefaultRearCamber = -1.5f,
                DefaultFrontWing = 2,
                DefaultRearWing = 3
            },
            ["Ferrari LaFerrari"] = new CarPreset
            {
                CarName = "Ferrari LaFerrari",
                CarClass = "Road",
                Power = 963,
                Weight = 1255,
                DriveType = "RWD",
                DefaultFrontSpring = 65.0f,
                DefaultRearSpring = 70.0f,
                DefaultFrontCamber = -2.2f,
                DefaultRearCamber = -1.8f,
                DefaultFrontWing = 1,
                DefaultRearWing = 2
            }
        };
    }

    private Dictionary<string, TrackPreset> InitializeTrackPresets()
    {
        // Real track data based on actual track layouts and characteristics
        return new Dictionary<string, TrackPreset>
        {
            ["Spa-Francorchamps"] = new TrackPreset
            {
                TrackName = "Spa-Francorchamps",
                LengthKm = 7.004f,
                TrackType = "Road",
                FastCornerCount = 12,
                SlowCornerCount = 7,
                LongestStraightKm = 2.0f, // Kemmel Straight
                AeroLevel = "Medium",
                SuspensionStiffness = "Medium"
            },
            ["Monza"] = new TrackPreset
            {
                TrackName = "Monza",
                LengthKm = 5.793f,
                TrackType = "Road",
                FastCornerCount = 8,
                SlowCornerCount = 3,
                LongestStraightKm = 1.2f, // Main straight
                AeroLevel = "Low", // High-speed configuration
                SuspensionStiffness = "Soft" // Kerb compliance needed
            },
            ["Mugello"] = new TrackPreset
            {
                TrackName = "Mugello",
                LengthKm = 5.245f,
                TrackType = "Road",
                FastCornerCount = 9,
                SlowCornerCount = 6,
                LongestStraightKm = 1.14f,
                AeroLevel = "High", // Fast flowing corners benefit from downforce
                SuspensionStiffness = "Stiff" // Significant elevation changes
            },
            ["Imola"] = new TrackPreset
            {
                TrackName = "Imola",
                LengthKm = 4.909f,
                TrackType = "Road",
                FastCornerCount = 11,
                SlowCornerCount = 6,
                LongestStraightKm = 0.8f,
                AeroLevel = "Medium",
                SuspensionStiffness = "Medium"
            },
            ["Nurburgring GP"] = new TrackPreset
            {
                TrackName = "Nurburgring GP",
                LengthKm = 5.148f,
                TrackType = "Road",
                FastCornerCount = 10,
                SlowCornerCount = 5,
                LongestStraightKm = 0.9f,
                AeroLevel = "Medium",
                SuspensionStiffness = "Medium"
            },
            ["Silverstone"] = new TrackPreset
            {
                TrackName = "Silverstone",
                LengthKm = 5.891f,
                TrackType = "Road",
                FastCornerCount = 14,
                SlowCornerCount = 4,
                LongestStraightKm = 0.77f,
                AeroLevel = "High", // Many high-speed corners (Maggots, Becketts)
                SuspensionStiffness = "Stiff"
            },
            ["Brands Hatch"] = new TrackPreset
            {
                TrackName = "Brands Hatch",
                LengthKm = 3.908f,
                TrackType = "Road",
                FastCornerCount = 6,
                SlowCornerCount = 3,
                LongestStraightKm = 0.5f,
                AeroLevel = "Medium",
                SuspensionStiffness = "Medium"
            },
            ["Red Bull Ring"] = new TrackPreset
            {
                TrackName = "Red Bull Ring",
                LengthKm = 4.318f,
                TrackType = "Road",
                FastCornerCount = 6,
                SlowCornerCount = 4,
                LongestStraightKm = 0.8f,
                AeroLevel = "Medium",
                SuspensionStiffness = "Stiff" // Significant elevation changes
            },
            ["Laguna Seca"] = new TrackPreset
            {
                TrackName = "Laguna Seca",
                LengthKm = 3.602f,
                TrackType = "Road",
                FastCornerCount = 7,
                SlowCornerCount = 4,
                LongestStraightKm = 0.5f,
                AeroLevel = "Medium",
                SuspensionStiffness = "Stiff" // Famous Corkscrew section
            },
            ["Vallelunga"] = new TrackPreset
            {
                TrackName = "Vallelunga",
                LengthKm = 4.085f,
                TrackType = "Road",
                FastCornerCount = 8,
                SlowCornerCount = 6,
                LongestStraightKm = 0.7f,
                AeroLevel = "Medium",
                SuspensionStiffness = "Medium"
            }
        };
    }
}
