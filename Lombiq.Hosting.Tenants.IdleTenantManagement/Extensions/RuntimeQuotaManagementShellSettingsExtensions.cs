using OrchardCore.Environment.Shell;
using System.Globalization;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Extensions;

public static class RuntimeQuotaManagementShellSettingsExtensions
{
    public static RuntimeQuotaShellSettings RuntimeQuotaSettings(this ShellSettings settings) =>
        new(settings);
}

public class RuntimeQuotaShellSettings
{
    private readonly ShellSettings _shellSettings;

    /// <summary>
    /// Gets or sets and sets the maximal time the tenant can be idle before being terminated, in minutes.
    /// </summary>
    public long MaxIdleMinutes
    {
        get => 1;
        set => _shellSettings["Lombiq.Hosting.Tenants.IdleTenantManagement.MaxIdleMinutes"] =
            value.ToString(CultureInfo.CurrentCulture.NumberFormat);
    }

    public RuntimeQuotaShellSettings(ShellSettings shellSettings) =>
        _shellSettings = shellSettings;
}
