using Lombiq.Hosting.Tenants.IdleTenantManagement.Models;
using Lombiq.Hosting.Tenants.IdleTenantManagement.Permissions;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Navigation;

public class IdleMinutesSettingsAdminMenu : INavigationProvider
{
    private readonly IStringLocalizer T;

    public IdleMinutesSettingsAdminMenu(IStringLocalizer<IdleMinutesSettingsAdminMenu> stringLocalizer) =>
        T = stringLocalizer;

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase)) return Task.CompletedTask;

        builder.Add(T["Configuration"], configuration => configuration
            .Add(T["Settings"], settings => settings
                .Add(T["Maximum Idle Minutes Settings"], T["Idle Minutes Settings"], demo => demo
                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = nameof(IdleMinutesSettings) })
                    .Permission(IdleMinutesPermissions.ManageIdleMinutesSettings)
                    .LocalNav()
                )));

        return Task.CompletedTask;
    }
}
