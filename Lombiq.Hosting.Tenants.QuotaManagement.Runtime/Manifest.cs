using OrchardCore.Modules.Manifest;
using static Lombiq.Hosting.Tenants.QuotaManagement.Runtime.Constants.FeatureNames;

[assembly: Feature(
    Id = DisableIdleTenants,
    Name = "Lombiq Hosting - Tenants Management - Disable Idle Tenants",
    Category = "Hosting",
    DefaultTenantOnly = true,
    Priority = "9999",
    Dependencies = new[] { "OrchardCore.Setup" }
)]
