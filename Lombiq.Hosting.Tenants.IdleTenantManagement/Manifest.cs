using OrchardCore.Modules.Manifest;
using static Lombiq.Hosting.Tenants.QuotaManagement.Runtime.Constants.FeatureNames;

[assembly: Module(
    Name = "Lombiq Hosting - Idle Tenant Management",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "0.0.1",
    Description = "Managing idle tenants."
)]

[assembly: Feature(
    Id = DisableIdleTenants,
    Name = "Lombiq Hosting - Idle Tenant Management - Disable Idle Tenants",
    Category = "Hosting",
    DefaultTenantOnly = true,
    Priority = "9999",
    Dependencies = new[] { "OrchardCore.Tenants" }
)]
