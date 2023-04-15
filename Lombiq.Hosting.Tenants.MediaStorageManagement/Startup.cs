using Lombiq.Hosting.Tenants.MediaStorageManagement.Filters;
using Lombiq.Hosting.Tenants.MediaStorageManagement.Service;
using Lombiq.Hosting.Tenants.MediaStorageManagement.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement;

public class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration) => _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<MediaStorageManagementOptions>(options =>
            _shellConfiguration
                .GetSection("Lombiq_Hosting_Tenants_MediaStorageManagement:Media_Storage_Management_Options")
                .Bind(options));

        services.AddScoped<IMediaQuoteService, MediaQuoteService>();

        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add(typeof(UploadFileSizeFilter));
            options.Conventions.Add(new DynamicMediaSizeApplicationModelConvention());
        });
    }
}
