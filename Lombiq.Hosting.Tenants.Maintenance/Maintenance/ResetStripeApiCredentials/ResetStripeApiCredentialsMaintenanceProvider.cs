using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Commerce.Payment.Stripe.Services;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ResetStripeApiCredentials;

public class ResetStripeApiCredentialsMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<ResetStripeApiCredentialsMaintenanceOptions> _options;
    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IShellFeaturesManager _shellFeaturesManager;

    public ResetStripeApiCredentialsMaintenanceProvider(
        IOptions<ResetStripeApiCredentialsMaintenanceOptions> options,
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        IShellFeaturesManager shellFeaturesManager)
    {
        _options = options;
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _shellFeaturesManager = shellFeaturesManager;
    }

    public override async Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        _options.Value.IsEnabled &&
        !context.WasLatestExecutionSuccessful() &&
        (await _shellFeaturesManager.GetEnabledFeaturesAsync()).Any(feature =>
            feature.Id == "OrchardCore.Commerce.Payment.Stripe");

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var settings = siteSettings.As<StripeApiSettings>();

        if (!settings.SecretKey.IsNullOrEmpty() ||
            !settings.PublishableKey.IsNullOrEmpty() ||
            !settings.WebhookSigningSecret.IsNullOrEmpty())
        {
            siteSettings.Alter<StripeApiSettings>(nameof(StripeApiSettings), settings =>
            {
                // These are publicly available test keys.
                settings.PublishableKey =
                    "pk_test_51H59owJmQoVhz82aWAoi9M5s8PC6sSAqFI7KfAD2NRKun5riDIOM0dvu2caM25a5f5JbYLMc5Umxw8Dl7dBIDN" + // #spell-check-ignore-line
                        "wM00yVbSX8uS"; // #spell-check-ignore-line

                var protector = _dataProtectionProvider.CreateProtector(nameof(StripeApiSettingsConfiguration));
                settings.SecretKey = protector
                    .Protect("sk_test_51H59owJmQoVhz82aOUNOuCVbK0u1zjyRFKkFp9EfrqzWaUWqQni3oSxljsdTIu2YZ9XvlbeGjZRU7" + // #spell-check-ignore-line
                        "B7ye2EjJQE000Dm2DtMWD"); // #spell-check-ignore-line

                settings.WebhookSigningSecret = string.Empty;
            });
        }
    }
}
