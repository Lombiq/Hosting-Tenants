using Lombiq.Hosting.Tenants.MediaStorageManagement.Filters;
using Lombiq.Hosting.Tenants.MediaStorageManagement.Service;
using Microsoft.AspNetCore.Mvc;
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
        services.AddScoped<IMediaQuoteService, MediaQuoteService>();

        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add(typeof(UploadFileSizeFilter));
            options.Conventions.Add(new DynamicMediaSizeApplicationModelConvention());
        });
    }
}
