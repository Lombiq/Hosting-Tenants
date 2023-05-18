using Lombiq.Hosting.Tenants.SearchBotBlocker.Constants;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lombiq Hosting - Tenants - Search Bot Blocker",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "0.0.1",
    Description = "Prevents search bots from indexing non-production environments by adding a noindex, nofollow meta tag.",
    Category = "Hosting"
)]

[assembly: Feature(
    Id = FeatureNames.SearchBotBlocker,
    Name = "Lombiq Hosting - Tenants - Search Bot Blocker",
    Category = "Hosting",
    IsAlwaysEnabled = true
)]
