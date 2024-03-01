using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Maintenance.ResetStripeApiCredentials;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Entities;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ResetStripeApiCredentials;

public class ResetStripeApiCredentialsMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<ResetStripeApiCredentialsMaintenanceOptions> _options;
    private readonly ISiteService _siteService;

    public ResetStripeApiCredentialsMaintenanceProvider(IOptions<ResetStripeApiCredentialsMaintenanceOptions> options, ISiteService siteService)
    {
        _options = options;
        _siteService = siteService;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.IsEnabled &&
            !context.WasLatestExecutionSuccessful());

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var settings = (await _siteService
            .GetSiteSettingsAsync())
            .As<StripeApiSettings>();

        if(settings.SecretKey.IsNullOrEmpty() && settings.PublishableKey.IsNullOrEmpty())
        {

        }
    }
}
