using OrchardCore.Modules.Manifest;
using static Lombiq.Hosting.Tenants.IdleTenantManagement.Constants.FeatureNames;

[assembly: Module(
    Name = "Lombiq Hosting - Idle Tenant Management",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "0.0.1",
    Description = "Managing idle tenants."
)]

[assembly: Feature(
    Id = ShutDownIdleTenants,
    Name = "Lombiq Hosting - Idle Tenant Management - Shut Down Idle Tenants",
    Description = "Shut down tenants not receiving requests after a configured amount of time to conserve computing resources.",
    Category = "Hosting",
    Priority = "9999",
    IsAlwaysEnabled = true
)]
