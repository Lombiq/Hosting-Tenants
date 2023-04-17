using Lombiq.Hosting.Tenants.MediaStorageManagement.Settings;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder PostConfigureMediaStorageManagementOptions(
        this OrchardCoreBuilder builder,
        long maximumStorageQuota)
    {
        builder.ConfigureServices((tenantServices, _) =>
            tenantServices.PostConfigure<MediaStorageManagementOptions>(options =>
                options.MaximumStorageQuota = maximumStorageQuota));

        return builder;
    }
}
