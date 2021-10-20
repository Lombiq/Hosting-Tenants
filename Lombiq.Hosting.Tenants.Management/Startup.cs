using Lombiq.Hosting.Tenants.Management.Constants;
using Lombiq.Hosting.Tenants.Management.Filters;
using Lombiq.Hosting.Tenants.Management.Service;
using Lombiq.Hosting.Tenants.Management.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Setup.Services;

namespace Lombiq.Hosting.Tenants.Management
{
    [Feature(FeatureNames.ForbindenTenantNames)]
    public class AsdasdStartup : StartupBase
    {
        private readonly IShellConfiguration _shellConfiguration;

        public AsdasdStartup(IShellConfiguration shellConfiguration) => _shellConfiguration = shellConfiguration;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ForbiddenTenantsOptions>(options =>
                _shellConfiguration
                    .GetSection("Lombiq_Hosting_Tenants_Management:Forbidden_Tenants_Options")
                    .Bind(options));

            services.Configure<MvcOptions>(options =>
                options.Filters.Add(typeof(ForbiddenTenantsFilter)));
        }
    }

    [Feature(FeatureNames.HideRecipesFromSetup)]
    public class SetupStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services) =>
            services.Replace(ServiceDescriptor.Scoped<ISetupService, SetupWithRecipesFilterService>());
    }
}
