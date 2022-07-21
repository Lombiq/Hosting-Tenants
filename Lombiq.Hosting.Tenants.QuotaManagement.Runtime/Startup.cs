using Lombiq.Hosting.Tenants.QuotaManagement.Runtime.Constants;
using Lombiq.Hosting.Tenants.QuotaManagement.Runtime.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;

namespace Lombiq.Hosting.Tenants.QuotaManagement.Runtime;

[Feature(FeatureNames.DisableIdleTenants)]
public class DisableIdleTenants : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddSingleton<IBackgroundTask, IdleShutdownTask>();
}