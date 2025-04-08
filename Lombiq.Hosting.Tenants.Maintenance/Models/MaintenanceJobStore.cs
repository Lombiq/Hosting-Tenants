using System.Collections.Concurrent;

namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public static class MaintenanceJobStore
{
    private static readonly ConcurrentDictionary<string, bool> _cancelFlags = new();

    public static void RequestCancel(string jobId) => _cancelFlags[jobId] = true;

    public static bool IsCancelled(string jobId) => _cancelFlags.TryGetValue(jobId, out var cancel) && cancel;

    public static void Clear(string jobId) => _cancelFlags.TryRemove(jobId, out _);
}
