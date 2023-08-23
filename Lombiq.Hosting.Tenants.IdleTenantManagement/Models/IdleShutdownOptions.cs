namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Models;

public class IdleShutdownOptions
{
    /// <summary>
    /// Gets or sets the maximum amount of idle time before the tenant shuts down.
    /// </summary>
    public int MaxIdleMinutes { get; set; } = 1;
}
