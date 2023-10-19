using Lombiq.Hosting.Tenants.EmailQuotaManagement.Constants;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Filters;
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
        services.AddSingleton<IBackgroundTask, EmailQuotaResetBackgroundTask>();

        services.Configure<EmailQuotaOptions>(options =>
            options.EmailQuotaPerMonth = _shellConfiguration.GetValue<int?>("Lombiq_Hosting_Tenants_EmailQuotaManagement:EmailQuotaPerMonth")
                ?? DefaultEmailQuota);

        services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(typeof(DashboardQuotaFilter));
                options.Filters.Add(typeof(EmailSettingsQuotaFilter));
            }
        );

        services.AddScoped<IEmailQuotaService, EmailQuotaService>();
        services.AddScoped<IEmailQuotaSubjectService, EmailQuotaSubjectService>();

        services.Decorate<ISmtpService, QuotaManagingSmtpServiceDecorator>();
    }
}
