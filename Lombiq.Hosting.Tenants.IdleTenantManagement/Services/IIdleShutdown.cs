using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Services;

/// <summary>
/// Service to shut down idle tenants.
/// </summary>
public interface IIdleShutdown
{
    /// <summary>
    /// Shuts down idle tenants if they are idle for more than the configured time.
    /// </summary>
    Task ShutDownIdleTenantsAsync();
}
