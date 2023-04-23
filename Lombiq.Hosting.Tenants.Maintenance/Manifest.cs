using OrchardCore.Modules.Manifest;
using static Lombiq.Hosting.Tenants.Maintenance.Constants.FeatureNames;

[assembly: Module(
    Name = "Lombiq Hosting - Tenants Maintenance",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "0.0.1"
)]

[assembly: Feature(
    Id = Maintenance,
    Name = "Lombiq Hosting - Tenants Maintenance",
    Description = "Provides maintenance operations for tenants.",
    Category = "Hosting",
    Dependencies = new[] { "OrchardCore.Tenants" }
)]

[assembly: Feature(
    Id = UpdateTenantUrl,
    Name = "Lombiq Hosting - Tenants Maintenance - Update Tenant URL",
    Description = "Updates the URL of the tenant (e.g., when the production database is copied to staging).",
    Category = "Maintenance",
    Dependencies = new[] { Maintenance }
)]
