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
    Id = UpdateSiteUrl,
    Name = "Lombiq Hosting - Tenants Maintenance - Update Site URL",
    Description = "Updates the URL of the site in the site settings (e.g., when the production database is copied to staging).",
    Category = "Maintenance",
    Dependencies = new[] { Maintenance }
)]

[assembly: Feature(
    Id = UpdateShellRequestUrls,
    Name = "Lombiq Hosting - Tenants Maintenance - Update Shell Request URLs",
    Description = "Updates the shell request URLs of each tenant (e.g., when the production database is copied to staging)." +
        " It's executed only on the default tenant.",
    Category = "Maintenance",
    DefaultTenantOnly = true,
    Dependencies = new[] { Maintenance }
)]
