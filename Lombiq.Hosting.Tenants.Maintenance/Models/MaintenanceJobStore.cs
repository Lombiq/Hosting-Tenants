using System.Collections.Concurrent;

namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public static class MaintenanceJobStore
{
    private static readonly ConcurrentDictionary<string, bool> _cancelFlags = new();

    public static bool RequestCancel(string jobId)
    {
        // Check if there is a job running.
        if (_cancelFlags.ContainsKey(jobId))
        {
            _cancelFlags[jobId] = true;
            return true;
        }

        return false;
    }

    public static bool IsCancelled(string jobId) => _cancelFlags.TryGetValue(jobId, out var cancel) && cancel;

    public static void Clear(string jobId) => _cancelFlags.AddOrUpdate(jobId, addValue: false, (_, _) => false);
}
