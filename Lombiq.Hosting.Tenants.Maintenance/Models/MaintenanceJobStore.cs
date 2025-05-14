using System.Collections.Concurrent;

namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public static class MaintenanceJobStore
{
    private static readonly ConcurrentDictionary<string, bool> _pauseFlags = new();

    public static bool RequestPause(string jobId)
    {
        // Check if there is a job running.
        if (_pauseFlags.ContainsKey(jobId))
        {
            _pauseFlags[jobId] = true;
            return true;
        }

        return false;
    }

    public static bool IsPaused(string jobId) => _pauseFlags.TryGetValue(jobId, out var pause) && pause;

    public static void Clear(string jobId) => _pauseFlags.AddOrUpdate(jobId, addValue: false, (_, _) => false);
}
