using Lombiq.Hosting.Tenants.FeaturesGuard.Constants;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lombiq Hosting - Tenants Health Checks",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "0.0.1",
    Description = "Multi-tenancy related health check improvements.",
    Category = "Hosting"
)]

[assembly: Feature(
    Id = FeatureNames.HealthChecks,
    Name = "Lombiq Hosting - Tenants Health Checks",
    Category = "Hosting",
    Dependencies = ["OrchardCore.HealthChecks"]
)]
