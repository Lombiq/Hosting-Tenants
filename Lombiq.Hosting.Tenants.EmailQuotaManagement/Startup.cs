using Lombiq.Hosting.Tenants.EmailQuotaManagement.Constants;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Indexes;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Email;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using YesSql.Indexes;
using static Lombiq.Hosting.Tenants.EmailQuotaManagement.Constants.EmailQuotaOptionsConstants;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement;

[Feature(FeatureNames.EmailQuotaManagement)]
public class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration) =>
        _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IIndexProvider, EmailQuotaIndexProvider>();
        services.Configure<EmailQuotaOptions>(options =>
            options.EmailQuota = _shellConfiguration.GetValue<int?>("Lombiq_Hosting_Tenants_EmailQuotaManagement:EmailQuota")
                ?? DefaultEmailQuota);

        services.AddScoped<IQuotaService, QuotaService>();
        services.Decorate<ISmtpService, EmailQuotaService>();
        services.AddSingleton<IBackgroundTask, EmailQuotaResetBackgroundTask>();
    }
}
