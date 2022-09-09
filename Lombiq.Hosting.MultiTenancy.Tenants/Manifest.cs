using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lombiq Hosting MultiTenancy Tenants",
    Author = "The Orchard Core Team",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "0.0.1",
    Description = "Adds useful features for multi-tenant applications.",
    Category = "Hosting"
)]

[assembly: Feature(
    Id = "FeaturesGuard",
    Name = "Lombiq Hosting - MultiTenancy - Features Guard",
    Category = "Hosting",
    IsAlwaysEnabled = true
)]
