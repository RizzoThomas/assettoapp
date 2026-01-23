using AssettoApp.Core.Interfaces;
using AssettoApp.Core.Models;
using System.Text.Json;

namespace AssettoApp.Core.Repositories;

/// <summary>
/// Repository for persisting session history to local JSON files
/// </summary>
public class SessionHistoryRepository : ISessionHistoryRepository
{
    private readonly string _storageDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public SessionHistoryRepository()
    {
        // Store in AppData/AssettoApp/Sessions
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _storageDirectory = Path.Combine(appDataPath, "AssettoApp", "Sessions");
        
        // Ensure directory exists
        Directory.CreateDirectory(_storageDirectory);
    }

    public async Task<bool> SaveSessionAsync(SessionHistory session)
    {
        try
        {
            var fileName = $"session_{session.SessionId:N}.json";
            var filePath = Path.Combine(_storageDirectory, fileName);
            
            var json = JsonSerializer.Serialize(session, JsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            
            return true;
        }
        catch (Exception ex)
        {
            // Log the error for debugging
            System.Diagnostics.Debug.WriteLine($"Failed to save session: {ex.Message}");
            return false;
        }
    }

    public async Task<SessionHistory?> LoadSessionAsync(Guid sessionId)
    {
        try
        {
            var fileName = $"session_{sessionId:N}.json";
            var filePath = Path.Combine(_storageDirectory, fileName);
            
            if (!File.Exists(filePath))
                return null;
            
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<SessionHistory>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load session {sessionId}: {ex.Message}");
            return null;
        }
    }

    public async Task<List<SessionHistory>> GetAllSessionsAsync()
    {
        var sessions = new List<SessionHistory>();
        
        try
        {
            var files = Directory.GetFiles(_storageDirectory, "session_*.json");
            
            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var session = JsonSerializer.Deserialize<SessionHistory>(json, JsonOptions);
                    if (session != null)
                        sessions.Add(session);
                }
                catch (Exception ex)
                {
                    // Skip corrupted files but log the issue
                    System.Diagnostics.Debug.WriteLine($"Failed to load session file {file}: {ex.Message}");
                }
            }
            
            // Sort by session start time, most recent first
            return sessions.OrderByDescending(s => s.SessionStart).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get all sessions: {ex.Message}");
            return sessions;
        }
    }

    public async Task<List<SessionHistory>> GetRecentSessionsAsync(int count = 10)
    {
        var allSessions = await GetAllSessionsAsync();
        return allSessions.Take(count).ToList();
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        try
        {
            var fileName = $"session_{sessionId:N}.json";
            var filePath = Path.Combine(_storageDirectory, fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to delete session {sessionId}: {ex.Message}");
            return false;
        }
    }

    public async Task<List<SessionHistory>> GetSessionsByCarAndTrackAsync(string carName, string trackName)
    {
        var allSessions = await GetAllSessionsAsync();
        return allSessions
            .Where(s => s.CarName.Equals(carName, StringComparison.OrdinalIgnoreCase) &&
                       s.TrackName.Equals(trackName, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
