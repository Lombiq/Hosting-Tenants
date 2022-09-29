using Lombiq.Hosting.Tenants.FeaturesGuard.Constants;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lombiq Hosting - Tenants - FeaturesGuard",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "0.0.1",
    Description = "Adds the FeaturesGuard feature.",
    Category = "Hosting"
)]

[assembly: Feature(
    Id = FeatureNames.FeaturesGuard,
    Name = "Lombiq Hosting - Tenants - FeaturesGuard",
    Category = "Hosting",
    IsAlwaysEnabled = true
)]
