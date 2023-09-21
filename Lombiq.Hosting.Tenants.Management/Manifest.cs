using OrchardCore.Modules.Manifest;
using static Lombiq.Hosting.Tenants.Management.Constants.FeatureNames;

[assembly: Module(
    Name = "Lombiq Hosting - Tenants Management",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "0.0.1",
    Description = "Manage restrictions on tenant creation."
)]

[assembly: Feature(
    Id = ForbiddenTenantNames,
    Name = "Lombiq Hosting - Tenants Management - Forbidden Tenant Names",
    Description = "Ability to configure hostnames that aren't allowed during tenant creation.",
    Category = "Hosting",
    DefaultTenantOnly = true,
    Dependencies = new[] { "OrchardCore.Tenants" }
)]

[assembly: Feature(
    Id = HideRecipesFromSetup,
    Name = "Lombiq Hosting - Tenants Management - Hide Recipes From Setup",
    Description = "Adds the ability to hide recipes from the setup screen based on configurable tags.",
    Category = "Hosting",
    DefaultTenantOnly = true,
    Dependencies = new[] { "OrchardCore.Setup" }
)]

[assembly: Feature(
    Id = ShellSettingsEditor,
    Name = "Lombiq Hosting - Tenants Management - Shell Settings Editor",
    Description = "",
    Category = "Hosting",
    DefaultTenantOnly = true,
    Dependencies = new[] { "OrchardCore.Tenants" }
)]
