using Lombiq.Hosting.MultiTenancy.Tenants.Constants;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lombiq Hosting Multi-Tenancy Tenants",
    Author = "The Orchard Core Team",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "0.0.1",
    Description = "Adds useful features for multi-tenant applications.",
    Category = "Hosting"
)]

[assembly: Feature(
    Id = FeatureNames.FeaturesGuard,
    Name = "Lombiq Hosting - Multi-Tenancy - Features Guard",
    Category = "Hosting",
    IsAlwaysEnabled = true
)]
