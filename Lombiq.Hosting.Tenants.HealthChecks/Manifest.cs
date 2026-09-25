using Lombiq.Hosting.Tenants.HealthChecks.Constants;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lombiq Hosting - Tenants Health Checks",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "0.0.1",
    Description = "Multi-tenancy related health check improvements.",
    Category = "Hosting"
)]

[assembly: Feature(
    Id = HealthChecksFeatureIds.AllTenants,
    Name = "Lombiq Hosting - Tenants Health Checks - All Tenants",
    Category = "Hosting",
    Dependencies = ["OrchardCore.HealthChecks"]
)]

[assembly: Feature(
    Id = HealthChecksFeatureIds.DefaultTenant,
    Name = "Lombiq Hosting - Tenants Health Checks - Default Tenant",
    Category = "Hosting",
    Dependencies = ["OrchardCore.HealthChecks"],
    DefaultTenantOnly = true
)]

[assembly: Feature(
    Id = HealthChecksFeatureIds.Admin,
    Name = "Lombiq Hosting - Tenants Health Checks - Admin Features",
    Category = "Hosting",
    Dependencies = [HealthChecksFeatureIds.DefaultTenant],
    DefaultTenantOnly = true
)]
