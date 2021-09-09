using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lombiq Hosting - Tenants Management",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "1.0.0",
    Description = "Manage restrictions on tenant creation.",
    Category = "Hosting",
    DefaultTenantOnly = true,
    Dependencies = new[] { "OrchardCore.Tenants" }
)]
