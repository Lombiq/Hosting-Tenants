using OrchardCore.Modules.Manifest;
using static Lombiq.Hosting.Tenants.MediaStorageManagement.Constants.FeatureNames;

[assembly: Module(
    Name = "Lombiq Hosting - Tenants Media Storage Management",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "0.0.1",
    Description = "Ability to configure storage quota for tenants."
)]

[assembly: Feature(
    Id = Module,
    Name = "Lombiq Hosting - Tenants Media Storage Management - Quota Management",
    Description = "Ability to configure storage quota for tenants.",
    Category = "Hosting",
    Dependencies = new[]
    {
        "OrchardCore.Media",
        "OrchardCore.DisplayManagement",
    }
)]
