using OrchardCore.Modules.Manifest;
using static Lombiq.Hosting.Tenants.Maintenance.Constants.FeatureNames;

[assembly: Module(
    Name = "Lombiq Hosting - Tenants Maintenance",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Hosting-Tenants",
    Version = "0.0.1"
)]

[assembly: Feature(
    Id = Maintenance,
    Name = "Lombiq Hosting - Tenants Maintenance",
    Description = "Provides maintenance operations for tenants.",
    Category = "Hosting",
    Dependencies = ["OrchardCore.Settings"]
)]

[assembly: Feature(
    Id = UpdateSiteUrl,
    Name = "Lombiq Hosting - Tenants Maintenance Update Site URL",
    Description = "Updates the URL of the site in the site settings (e.g., when the production database is copied to staging).",
    Category = "Maintenance",
    Dependencies = [Maintenance]
)]

[assembly: Feature(
    Id = UpdateShellRequestUrls,
    Name = "Lombiq Hosting - Tenants Maintenance Update Shell Request URLs",
    Description = "Updates the shell request URLs of each tenant (e.g., when the production database is copied to staging)." +
        " It's executed only on the default tenant.",
    Category = "Maintenance",
    DefaultTenantOnly = true,
    Dependencies = [Maintenance]
)]

[assembly: Feature(
    Id = AddAdministratorRoleToUsersWithRole,
    Name = "Lombiq Hosting - Tenants Maintenance Add Administrator Role to Users With Role",
    Description = "Adds the Administrator role to users with the configured role (e.g., when the production database " +
        "is copied to staging).",
    Category = "Maintenance",
    Dependencies = [Maintenance, "OrchardCore.Users"]
)]

[assembly: Feature(
    Id = RemoveUsers,
    Name = "Lombiq Hosting - Tenants Maintenance Remove Users",
    Description = "Removes users with the configured email domain.",
    Category = "Maintenance",
    Dependencies = [Maintenance]
)]

[assembly: Feature(
    Id = ChangeUserSensitiveContent,
    Name = "Lombiq Hosting - Tenants Maintenance Change User Sensitive Content",
    Description = "Replaces the users' username, email and password with realistic but random values.",
    Category = "Maintenance",
    Dependencies = [Maintenance]
)]

[assembly: Feature(
    Id = DeleteOrRebuildElasticsearchIndices,
    Name = "Lombiq Hosting - Tenants Maintenance Delete Elasticsearch Indexes",
    Description = "Deletes Elasticsearch indexes.",
    Category = "Maintenance",
    Dependencies = [Maintenance, "OrchardCore.Search.Elasticsearch"]
)]

[assembly: Feature(
    Id = DeleteElasticsearchIndicesBeforeSetup,
    Name = "Lombiq Hosting - Tenants Maintenance Delete Elasticsearch Indexes Before Setup",
    Description = "Deletes Elasticsearch indexes before setup.",
    Category = "Maintenance",
    Dependencies = []
)]

[assembly: Feature(
    Id = PurgeMediaCache,
    Name = "Lombiq Hosting - Tenants Maintenance Purge Media Cache",
    Description = "Purges the local cache for remote media files to avoid stale assets during production-staging swaps.",
    Category = "Maintenance",
    Dependencies = [Maintenance, "OrchardCore.Media"]
)]

[assembly: Feature(
    Id = UpdateEnabledFeatures,
    Name = "Lombiq Hosting - Tenants Maintenance Update Enabled Features",
    Description = "Updates the enabled features of tenants.",
    Category = "Maintenance",
    Dependencies = []
)]

[assembly: Feature(
    Id = StaggeredTenantWakeUp,
    Name = "Lombiq Hosting - Tenants Maintenance Staggered Tenant Wake-up",
    Description = "Creates a scope for all running tenants and this way all migrations and maintenances are triggered to run.",
    Category = "Maintenance",
    DefaultTenantOnly = true,
    Dependencies = [
        Maintenance,
        "OrchardCore.Tenants",
        "OrchardCore.ContentFields",
        "OrchardCore.Contents",
    ]
)]
