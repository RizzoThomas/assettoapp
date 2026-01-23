namespace AssettoApp.Core.Models;

/// <summary>
/// Complete session history with all laps and statistics
/// </summary>
public class SessionHistory
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public DateTime SessionStart { get; set; }
    public DateTime? SessionEnd { get; set; }
    
    public GameType GameType { get; set; }
    public string CarName { get; set; } = string.Empty;
    public string TrackName { get; set; } = string.Empty;
    
    public List<LapData> Laps { get; set; } = new();
    
    // Session statistics
    public int TotalLaps => Laps.Count;
    public int ValidLaps => Laps.Count(l => l.IsValid);
    public int PerfectLaps => Laps.Count(l => l.IsPerfectLap);
    public int TotalTrackLimitViolations => Laps.Sum(l => l.TrackLimitViolations);
    
    public LapData? BestLap => Laps.Where(l => l.IsValid).OrderBy(l => l.LapTime).FirstOrDefault();
    public TimeSpan? BestLapTime => BestLap?.LapTime;
    
    // Tire wear statistics
    public float TotalTireWear
    {
        get
        {
            if (Laps.Count == 0) return 0;
            var firstLap = Laps.First();
            var lastLap = Laps.Last();
            if (firstLap.TireWearStart == null || lastLap.TireWearEnd == null) return 0;
            return lastLap.TireWearEnd.AverageWear - firstLap.TireWearStart.AverageWear;
        }
    }
    
    // Fuel statistics
    public float TotalFuelConsumed => Laps.Sum(l => l.FuelConsumed);
    public float AverageFuelPerLap => TotalLaps > 0 ? TotalFuelConsumed / TotalLaps : 0;
    
    // Performance trends
    public List<TimeSpan> LapTimes => Laps.Where(l => l.IsValid).Select(l => l.LapTime).ToList();
    public bool IsImprovingPace
    {
        get
        {
            if (LapTimes.Count < 3) return false;
            var lastThree = LapTimes.TakeLast(3).ToList();
            return lastThree[2] < lastThree[0];  // Last lap faster than third-to-last
        }
    }
}
