namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Models;

public class IdleMinutesOptions
{
    /// <summary>
    /// Gets or sets the maximum amount of time before the tenant shuts down.
    /// </summary>
    public int MaxIdleMinutes { get; set; } = 1;
}
