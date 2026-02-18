using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Search.Elasticsearch;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance;

// This can be removed if https://github.com/OrchardCMS/OrchardCore/pull/18855 is merged.
public sealed class BugFixMigrations : DataMigration
{
    private const string PermissionNamePrefix = "QueryElasticsearch";
    private const string PermissionNameSuffix = "Index";

    private readonly ShellSettings _shellSettings;

    public BugFixMigrations(ShellSettings shellSettings) =>
        _shellSettings = shellSettings;

    public int Create()
    {
        if (!_shellSettings.IsInitializing())
        {
            ShellScope.AddDeferredTask(ReplaceObsoletePermissionsAsync);
        }

        return 1;
    }

    internal static bool IsElasticsearchIndexPermissionClaim(RoleClaim claim) =>
        claim.ClaimType == nameof(Permission) &&
        claim.ClaimValue.StartsWithOrdinalIgnoreCase(PermissionNamePrefix) &&
        claim.ClaimValue.EndsWithOrdinalIgnoreCase(PermissionNameSuffix);

    internal static string GetIndexNameFromPermissionName(string permissionName) =>
        permissionName[PermissionNamePrefix.Length..^PermissionNameSuffix.Length];

    /// <summary>
    /// Selects the roles that need to be updated, and replaces their <c>QueryElasticsearch{0}Index</c> permissions with
    /// the equivalent <c>QueryIndex_{0}</c> permissions.
    /// </summary>
    private static async Task ReplaceObsoletePermissionsAsync(ShellScope shellScope)
    {
        var indexProfileManager = shellScope.ServiceProvider.GetRequiredService<IIndexProfileManager>();
        var roleService = shellScope.ServiceProvider.GetRequiredService<IRoleService>();
        var roleStore = shellScope.ServiceProvider.GetRequiredService<IRoleStore<IRole>>();

        var allRoles = await roleService.GetRolesAsync();
        var rolesToUpdate = allRoles
            .Where(role => role is Role)
            .Cast<Role>()
            .Where(role => role.RoleClaims.Any(IsElasticsearchIndexPermissionClaim))
            .ToList();

        foreach (var role in rolesToUpdate)
        {
            foreach (var claim in role.RoleClaims.Where(IsElasticsearchIndexPermissionClaim))
            {
                var name = GetIndexNameFromPermissionName(claim.ClaimValue);
                var indexProfile = await indexProfileManager.FindByNameAndProviderAsync(
                    name,
                    ElasticsearchConstants.ProviderName);

                if (indexProfile != null)
                {
                    claim.ClaimValue = IndexingPermissions.CreateDynamicPermission(indexProfile).Name;
                }
            }

            await roleStore.UpdateAsync(role, CancellationToken.None);
        }
    }
}
