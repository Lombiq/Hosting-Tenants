using Lombiq.HelpfulLibraries.OrchardCore.Navigation;
using Lombiq.Hosting.Tenants.Maintenance.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Navigation;

namespace Lombiq.Hosting.Tenants.Maintenance;

public sealed class AdminMenu : AdminMenuNavigationProviderBase
{
    private readonly ShellSettings _shellSettings;

    public AdminMenu(IHttpContextAccessor hca, IStringLocalizer<AdminMenu> stringLocalizer, ShellSettings shellSettings)
        : base(hca, stringLocalizer) => _shellSettings = shellSettings;

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
                .Add(T["Staggered Tenant Wake-Up"], T["Staggered Tenant Wake-Up"].PrefixPosition(), featureProfiles => featureProfiles
                    .ActionTask<StaggeredTenantsWakeUpAdminController>(_hca.HttpContext, controller => controller.Index())
                    .LocalNav())
            );
    }
}
