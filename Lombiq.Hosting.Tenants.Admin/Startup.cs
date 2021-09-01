using Lombiq.Hosting.Tenants.Admin.Constants;
using Lombiq.Hosting.Tenants.Admin.Filters;
using Lombiq.Hosting.Tenants.Admin.Permissions;
using Lombiq.Hosting.Tenants.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;

namespace Lombiq.Hosting.Tenants.Admin
{
    [Feature(FeatureNames.Module)]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MvcOptions>(options => options.Filters.Add(typeof(TenantsIndexFilter)));
            services.AddScoped<IPermissionProvider, TenantAdminPermissions>();
            services.AddSingleton<ITenantLoginPasswordValidator, TenantLoginKeyValidator>();
        }
    }
}
