using Lombiq.Hosting.Tenants.IdleTenantManagement.Models;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;
using System.Globalization;

namespace Lombiq.Hosting.Tenants.IdleTenantManagement.Services;

public class IdleMinutesSettingsConfiguration : IConfigureOptions<IdleMinutesSettings>
{
    private readonly ISiteService _siteService;

    public IdleMinutesSettingsConfiguration(ISiteService siteService) =>
        _siteService = siteService;

    public void Configure(IdleMinutesSettings options)
    {
        if (long.Parse(options.MaxIdleMinutes, CultureInfo.InvariantCulture.NumberFormat) <= 0) return;

        var settings = _siteService.GetSiteSettingsAsync()
            .GetAwaiter()
            .GetResult()
            .As<IdleMinutesSettings>();

        options.MaxIdleMinutes = settings.MaxIdleMinutes;
    }
}
