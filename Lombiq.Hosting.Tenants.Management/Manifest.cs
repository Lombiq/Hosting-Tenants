using OrchardCore.Modules.Manifest;
using static Lombiq.Hosting.Tenants.Management.Constants.FeatureNames;

[assembly: Module(
    Name = "Lombiq Hosting - Tenants Management",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "1.0.0",
    Description = "Manage restrictions on tenant creation."
)]

[assembly: Feature(
    Id = ForbindenTenantName,
    Name = "Lombiq Hosting - Tenants Management - Forbinden Tenant Names",
    Description = "Ability to configure hostnames that don't allow during tenant creation.",
    Category = "Hosting",
    DefaultTenantOnly = true,
    Dependencies = new[] { "OrchardCore.Tenants" }
)]

[assembly: Feature(
    Id = HideRecipesFromSetup,
    Name = "Lombiq Hosting - Tenants Management - Hide Recipes From Setup",
    Description = "Ability to configure recipe categories witch won't be available during tenant setup.",
    Category = "Hosting",
    DefaultTenantOnly = true,
    Dependencies = new[] { "OrchardCore.Setup" }
)]
