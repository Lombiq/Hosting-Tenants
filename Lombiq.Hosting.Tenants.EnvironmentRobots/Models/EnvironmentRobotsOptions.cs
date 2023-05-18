using Microsoft.Extensions.Hosting;

namespace Lombiq.Hosting.Tenants.EnvironmentRobots.Models;

/// <summary>
/// Further configuration options for the module.
/// </summary>
public class EnvironmentRobotsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to add a noindex, nofollow meta tag when the app is not running in a
    /// production environment. It adds the tags when <see cref="IsProduction"/> is <see langword="false"/>. When set
    /// it overrides <see cref="HostEnvironmentEnvExtensions.IsProduction"/>'s result.
    /// </summary>
    public bool? IsProduction { get; set; }
}
