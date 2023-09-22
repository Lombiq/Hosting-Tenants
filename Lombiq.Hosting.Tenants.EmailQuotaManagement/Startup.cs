using Lombiq.Hosting.Tenants.EmailQuotaManagement.Constants;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Filters;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Indexes;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Migrations;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Data.Migration;
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
        services.AddDataMigration<EmailQuotaMigrations>();
        services.AddSingleton<IIndexProvider, EmailQuotaIndexProvider>();
        services.Configure<EmailQuotaOptions>(options =>
            options.EmailQuota = _shellConfiguration.GetValue<int?>("Lombiq_Hosting_Tenants_EmailQuotaManagement:EmailQuota")
                ?? DefaultEmailQuota);

        services.AddScoped<IQuotaService, QuotaService>();
        services.Decorate<ISmtpService, EmailSenderQuotaService>();
        services.AddSingleton<IBackgroundTask, EmailQuotaResetBackgroundTask>();

        services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(typeof(EmailQuotaErrorFilter));
                options.Filters.Add(typeof(EmailSettingsQuotaFilter));
            }
        );

        services.AddScoped<IEmailQuotaEmailService, EmailQuotaEmailService>();
    }
}
