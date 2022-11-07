using Lombiq.Hosting.Tenants.FeaturesGuard.Constants;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lombiq Hosting - Tenants - Features Guard",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "0.0.1",
    Description = "Prevents disabling a configurable set of features on tenants.",
    Category = "Hosting"
)]

[assembly: Feature(
    Id = FeatureNames.FeaturesGuard,
    Name = "Lombiq Hosting - Tenants - Features Guard",
    Category = "Hosting",
    IsAlwaysEnabled = true
)]
