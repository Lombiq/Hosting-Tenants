using Lombiq.Hosting.Tenants.IdleTenantManagement.Models;
using Lombiq.Hosting.Tenants.IdleTenantManagement.Permissions;
using Lombiq.Hosting.Tenants.IdleTenantManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Drivers;

public class IdleMinutesSettingsDisplayDriver : SectionDisplayDriver<ISite, IdleMinutesSettings>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _hca;

    public IdleMinutesSettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _hca = httpContextAccessor;
    }

    public override async Task<IDisplayResult> EditAsync(IdleMinutesSettings section, BuildEditorContext context)
    {
        if (!await IsAuthorizedToManageIdleMinutesSettingsAsync())
        {
            return null;
        }

        return Initialize<IdleMinutesSettingsViewModel>(
                $"{nameof(IdleMinutesSettings)}_Edit",
                viewModel =>
                    viewModel.MaxIdleMinutes = section.MaxIdleMinutes)
            .PlaceInContent(1)
            .OnGroup(nameof(IdleMinutesSettings));
    }

    public override async Task<IDisplayResult> UpdateAsync(IdleMinutesSettings section, BuildEditorContext context)
    {
        if (!await IsAuthorizedToManageIdleMinutesSettingsAsync())
        {
            return null;
        }

        if (context.GroupId == nameof(IdleMinutesSettings))
        {
            var viewModel = new IdleMinutesSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

            section.MaxIdleMinutes = viewModel.MaxIdleMinutes;
        }

        return await EditAsync(section, context);
    }

    private async Task<bool> IsAuthorizedToManageIdleMinutesSettingsAsync()
    {
        var user = _hca.HttpContext?.User;

        return user != null &&
               await _authorizationService.AuthorizeAsync(user, IdleMinutesPermissions.ManageIdleMinutesSettings);
    }
}
