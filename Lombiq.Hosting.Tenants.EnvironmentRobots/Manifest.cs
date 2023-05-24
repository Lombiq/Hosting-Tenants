using Lombiq.Hosting.Tenants.EnvironmentRobots.Constants;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lombiq Hosting - Tenants - Environment Robots",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "0.0.1",
    Description = "Prevents search bots from indexing non-production environments by adding a meta tag and a response header with noindex, nofollow values.",
    Category = "Hosting"
)]

[assembly: Feature(
    Id = FeatureNames.EnvironmentRobots,
    Name = "Lombiq Hosting - Tenants - Environment Robots",
    Category = "Hosting",
    IsAlwaysEnabled = true
)]
