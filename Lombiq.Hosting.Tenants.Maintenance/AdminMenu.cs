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

    protected override void Build(NavigationBuilder builder) =>
        builder
            .Add(T["Tools"], tenancy => tenancy
                .Add(T["Maintenance Task Executions"], T["Maintenance Task Executions"].PrefixPosition(), executions => executions
                    .ActionTask<MaintenanceTaskController>(_hca.HttpContext, controller => controller.Index())
                    .Id("maintenance-task-executions")
                    .LocalNav())
            );
}
