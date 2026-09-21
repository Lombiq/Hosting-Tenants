using Lombiq.HelpfulLibraries.OrchardCore.Navigation;
using Lombiq.Hosting.Tenants.HealthChecks;
using Lombiq.Hosting.Tenants.HealthChecks.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Navigation;

namespace Lombiq.Hosting.Tenants.Management;

public sealed class AdminMenu : AdminMenuNavigationProviderBase
{
    private readonly ShellSettings _shellSettings;

    public AdminMenu(IHttpContextAccessor hca, ShellSettings shellSettings, IStringLocalizer stringLocalizer)
        : base(hca, stringLocalizer) =>
        _shellSettings = shellSettings;

    protected override void Build(NavigationBuilder builder)
    {
        // Don't add the menu item on non-default tenants.
        if (!_shellSettings.IsDefaultShell())
        {
            return;
        }

        builder
            .Add(T["Multi-Tenancy"], tenancy => tenancy
                .AddClass("menu-multitenancy")
                .Add(T["Health Checks"], T["Health Checks"].PrefixPosition(), healthChecks => healthChecks
                    .Permission(HealthChecksPermissions.ViewHealthChecks)
                    .ActionTask<AdminController>(_hca.HttpContext, controller => controller.Index())
                    .Id("health-checks")
                    .LocalNav())
            );
    }
}
