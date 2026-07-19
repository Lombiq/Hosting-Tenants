using Lombiq.Hosting.Tenants.MediaStorageManagement.Filters;
using Lombiq.Hosting.Tenants.MediaStorageManagement.Handlers;
using Lombiq.Hosting.Tenants.MediaStorageManagement.Service;
using Lombiq.Hosting.Tenants.MediaStorageManagement.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Media.Events;
using OrchardCore.Modules;
using static Lombiq.Hosting.Tenants.MediaStorageManagement.Constants.MediaStorageManagementOptionsConstants;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration) => _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        var maximumStorageQuotaBytes =
            _shellConfiguration.GetValue<long?>(
                "Lombiq_Hosting_Tenants_MediaStorageManagement:MaximumStorageQuotaBytes") ??
            _shellConfiguration.GetValue<long?>(
                "Lombiq_Hosting_Tenants_MediaStorageManagement:Media_Storage_Management_Options:MaximumSpace");
        services.Configure<MediaStorageManagementOptions>(options =>
            options.MaximumStorageQuotaBytes = maximumStorageQuotaBytes ?? MaximumStorageQuotaBytes);

        services.AddScoped<IMediaStorageQuotaService, MediaStorageQuotaService>();
        services.AddSingleton<IMediaEventHandler, MediaStorageQuotaHandler>();

        services.Configure<MvcOptions>(options => options.Filters.Add<UploadFileSizeShapeFilter>());
    }
}
