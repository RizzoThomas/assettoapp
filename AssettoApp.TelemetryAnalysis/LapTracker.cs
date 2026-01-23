using AssettoApp.Core.Interfaces;
using AssettoApp.Core.Models;

namespace AssettoApp.TelemetryAnalysis;

/// <summary>
/// Tracks laps in real-time from telemetry data
/// </summary>
public class LapTracker : ILapTracker
{
    private SessionHistory? _currentSession;
    private LapData? _currentLap;
    private int _lastKnownLap = 0;
    private TelemetryData? _lastTelemetry;
    private List<TelemetryData> _currentLapTelemetry = new();
    
    public SessionHistory? CurrentSession => _currentSession;

    public void StartSession(GameType gameType, string carName, string trackName)
    {
        _currentSession = new SessionHistory
        {
            SessionId = Guid.NewGuid(),
            SessionStart = DateTime.UtcNow,
            GameType = gameType,
            CarName = carName,
            TrackName = trackName
        };
        
        _lastKnownLap = 0;
        _currentLap = null;
        _lastTelemetry = null;
        _currentLapTelemetry.Clear();
    }

    public void ProcessTelemetry(TelemetryData telemetry)
    {
        if (_currentSession == null)
            return;
        
        // Store telemetry for current lap analysis
        _currentLapTelemetry.Add(telemetry);
        
        // Detect lap change
        if (telemetry.CurrentLap > _lastKnownLap)
        {
            // Complete the previous lap
            if (_currentLap != null && _lastTelemetry != null)
            {
                CompleteLap(_lastTelemetry);
            }
            
            // Start new lap
            StartNewLap(telemetry);
            _lastKnownLap = telemetry.CurrentLap;
        }
        
        // Update current lap data
        if (_currentLap != null)
        {
            UpdateCurrentLap(telemetry);
        }
        
        _lastTelemetry = telemetry;
    }

    private void StartNewLap(TelemetryData telemetry)
    {
        _currentLapTelemetry.Clear();
        
        _currentLap = new LapData
        {
            LapNumber = telemetry.CurrentLap,
            StartTime = DateTime.UtcNow,
            IsValid = telemetry.IsLapValid,
            TrackLimitViolations = 0,
            TrackTemperature = telemetry.TrackTemperature,
            AirTemperature = telemetry.AirTemperature,
            TrackGrip = telemetry.TrackGrip,
            FuelAtStart = telemetry.Fuel,
            TireWearStart = telemetry.Tires != null && telemetry.Tires.Length == 4 ? new TireWearSnapshot
            {
                Timestamp = DateTime.UtcNow,
                FrontLeftWear = telemetry.Tires[0]?.Wear ?? 0,
                FrontRightWear = telemetry.Tires[1]?.Wear ?? 0,
                RearLeftWear = telemetry.Tires[2]?.Wear ?? 0,
                RearRightWear = telemetry.Tires[3]?.Wear ?? 0,
                FrontLeftTemp = telemetry.Tires[0]?.Temperature ?? 0,
                FrontRightTemp = telemetry.Tires[1]?.Temperature ?? 0,
                RearLeftTemp = telemetry.Tires[2]?.Temperature ?? 0,
                RearRightTemp = telemetry.Tires[3]?.Temperature ?? 0
            } : null
        };
    }

    private void UpdateCurrentLap(TelemetryData telemetry)
    {
        if (_currentLap == null) return;
        
        // Update lap validity
        _currentLap.IsValid = telemetry.IsLapValid;
        _currentLap.TrackLimitViolations = telemetry.TrackLimitViolations;
        
        // Calculate current lap time
        _currentLap.LapTime = telemetry.CurrentLapTime;
    }

    private void CompleteLap(TelemetryData finalTelemetry)
    {
        if (_currentLap == null) return;
        
        _currentLap.EndTime = DateTime.UtcNow;
        _currentLap.FuelAtEnd = finalTelemetry.Fuel;
        
        // Calculate tire wear at lap end
        _currentLap.TireWearEnd = finalTelemetry.Tires != null && finalTelemetry.Tires.Length == 4 ? new TireWearSnapshot
        {
            Timestamp = DateTime.UtcNow,
            FrontLeftWear = finalTelemetry.Tires[0]?.Wear ?? 0,
            FrontRightWear = finalTelemetry.Tires[1]?.Wear ?? 0,
            RearLeftWear = finalTelemetry.Tires[2]?.Wear ?? 0,
            RearRightWear = finalTelemetry.Tires[3]?.Wear ?? 0,
            FrontLeftTemp = finalTelemetry.Tires[0]?.Temperature ?? 0,
            FrontRightTemp = finalTelemetry.Tires[1]?.Temperature ?? 0,
            RearLeftTemp = finalTelemetry.Tires[2]?.Temperature ?? 0,
            RearRightTemp = finalTelemetry.Tires[3]?.Temperature ?? 0
        } : null;
        
        // Calculate lap statistics from telemetry
        if (_currentLapTelemetry.Count > 0)
        {
            _currentLap.AverageSpeed = _currentLapTelemetry.Average(t => t.SpeedKmh);
            _currentLap.MaxSpeed = _currentLapTelemetry.Max(t => t.SpeedKmh);
            _currentLap.AverageThrottle = _currentLapTelemetry.Average(t => t.Throttle);
            _currentLap.AverageBrake = _currentLapTelemetry.Average(t => t.Brake);
        }
        
        // Determine if lap is perfect
        _currentLap.IsPerfectLap = IsPerfectLap(_currentLap);
        
        // Add lap to session
        _currentSession?.Laps.Add(_currentLap);
        
        _currentLap = null;
    }

    public SessionHistory? EndSession()
    {
        if (_currentSession == null)
            return null;
        
        // Complete any lap in progress
        if (_currentLap != null && _lastTelemetry != null)
        {
            CompleteLap(_lastTelemetry);
        }
        
        _currentSession.SessionEnd = DateTime.UtcNow;
        
        var session = _currentSession;
        _currentSession = null;
        _currentLap = null;
        _lastKnownLap = 0;
        _lastTelemetry = null;
        _currentLapTelemetry.Clear();
        
        return session;
    }

    public LapData? GetCurrentLap()
    {
        return _currentLap;
    }

    public bool IsPerfectLap(LapData lap)
    {
        // A perfect lap must:
        // 1. Be valid (no track limits)
        // 2. Have no track limit violations
        // 3. Have consistent pace (not too slow)
        
        if (!lap.IsValid || lap.TrackLimitViolations > 0)
            return false;
        
        // Check if lap time is within reasonable bounds
        // (not significantly slower than average, which would indicate mistakes)
        if (_currentSession != null && _currentSession.Laps.Count > 3)
        {
            var validLaps = _currentSession.Laps.Where(l => l.IsValid).ToList();
            if (validLaps.Count >= 3)
            {
                var avgTime = TimeSpan.FromMilliseconds(validLaps.Average(l => l.LapTime.TotalMilliseconds));
                var tolerance = avgTime.TotalMilliseconds * 0.03; // 3% tolerance
                
                // Lap should not be more than 3% slower than average
                if (lap.LapTime.TotalMilliseconds > avgTime.TotalMilliseconds + tolerance)
                    return false;
            }
        }
        
        return true;
    }
}
