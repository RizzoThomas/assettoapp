using AssettoApp.Core.Models;

namespace AssettoApp.Core.Interfaces;

/// <summary>
/// Interface for persisting and retrieving session history
/// </summary>
public interface ISessionHistoryRepository
{
    /// <summary>
    /// Save a session to persistent storage
    /// </summary>
    Task<bool> SaveSessionAsync(SessionHistory session);
    
    /// <summary>
    /// Load a specific session by ID
    /// </summary>
    Task<SessionHistory?> LoadSessionAsync(Guid sessionId);
    
    /// <summary>
    /// Get all saved sessions
    /// </summary>
    Task<List<SessionHistory>> GetAllSessionsAsync();
    
    /// <summary>
    /// Get recent sessions (limited number)
    /// </summary>
    Task<List<SessionHistory>> GetRecentSessionsAsync(int count = 10);
    
    /// <summary>
    /// Delete a session
    /// </summary>
    Task<bool> DeleteSessionAsync(Guid sessionId);
    
    /// <summary>
    /// Get sessions for a specific car/track combination
    /// </summary>
    Task<List<SessionHistory>> GetSessionsByCarAndTrackAsync(string carName, string trackName);
}
