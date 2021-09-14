using Lombiq.Hosting.Tenants.Management.Filters;
using Lombiq.Hosting.Tenants.Management.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.Management
{
    public class Startup : StartupBase
    {
        private readonly IShellConfiguration _shellConfiguration;

        public Startup(IShellConfiguration shellConfiguration) => _shellConfiguration = shellConfiguration;

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
}
