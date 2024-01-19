using Lombiq.Hosting.Tenants.EmailQuotaManagement.Constants;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lombiq Hosting - Tenants Email Quota Management",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "0.0.1",
    Description = "Allows setting email quotas for tenants.",
    Category = "Hosting"
)]

[assembly: Feature(
    Id = FeatureNames.EmailQuotaManagement,
    Name = "Lombiq Hosting - Tenants Email Quota Management",
    Category = "Hosting",
    IsAlwaysEnabled = true,
    Dependencies =
    [
        "OrchardCore.Emails",
        "Lombiq.HelpfulExtensions.Emails",
    ]
)]
