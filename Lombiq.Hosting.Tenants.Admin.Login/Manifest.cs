using OrchardCore.Modules.Manifest;
using static Lombiq.Hosting.Tenants.Admin.Login.Constants.FeatureNames;

[assembly: Module(
    Name = "Lombiq Hosting - Tenants Admin Login ",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "1.0.0"
)]

[assembly: Feature(
    Id = Module,
    Name = "Lombiq Hosting - Tenants Admin Login",
    Description = "Ability to log in as a tenant’s admin user.",
    Category = "Hosting",
    DefaultTenantOnly = true,
    Dependencies = new[] { "OrchardCore.Tenants" }
)]

[assembly: Feature(
    Id = SubTenant,
    Name = "Lombiq Hosting - Tenants Admin Login - Sub-tenant",
    Description = "Adds the ability to log in to the tenant from the Default tenant.",
    Category = "Hosting",
    IsAlwaysEnabled = true
)]
