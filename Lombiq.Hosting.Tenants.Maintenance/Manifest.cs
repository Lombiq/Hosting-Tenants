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
    Name = "Lombiq Hosting - Tenants Maintenance Update Site URL",
    Description = "Updates the URL of the site in the site settings (e.g., when the production database is copied to staging).",
    Category = "Maintenance",
    Dependencies = new[] { Maintenance }
)]

[assembly: Feature(
    Id = UpdateShellRequestUrls,
    Name = "Lombiq Hosting - Tenants Maintenance Update Shell Request URLs",
    Description = "Updates the shell request URLs of each tenant (e.g., when the production database is copied to staging)." +
        " It's executed only on the default tenant.",
    Category = "Maintenance",
    DefaultTenantOnly = true,
    Dependencies = new[] { Maintenance }
)]

[assembly: Feature(
    Id = AddSiteOwnerPermissionToRole,
    Name = "Lombiq Hosting - Tenants Maintenance Add Site Owner Permission To Role",
    Description = "Adds the Site Owner permission to a role (e.g., when the production database is copied to staging).",
    Category = "Maintenance",
    DefaultTenantOnly = true,
    Dependencies = new[] { Maintenance }
)]

[assembly: Feature(
    Id = RemoveUsers,
    Name = "Lombiq Hosting - Tenants Maintenance Remove Users",
    Description = "Removes users with the configured email domain.",
    Category = "Maintenance",
    DefaultTenantOnly = true,
    Dependencies = new[] { Maintenance }
)]

[assembly: Feature(
    Id = ChangeUserSensitiveContent,
    Name = "Lombiq Hosting - Tenants Maintenance Change User Sensitive Content",
    Description = "Replaces the users' username, email and password with realistic but random values.",
    Category = "Maintenance",
    DefaultTenantOnly = true,
    Dependencies = new[] { Maintenance }
)]

[assembly: Feature(
    Id = ResetStripeApiCredentials,
    Name = "Lombiq Hosting - Tenants Maintenance Reset Stripe API Credentials",
    Description = "Replaces the Stripe Publishable Key and Secret Key to the publicly available test keys, if they are empty.",
    Category = "Maintenance",
    Dependencies = new[] { Maintenance }
)]
