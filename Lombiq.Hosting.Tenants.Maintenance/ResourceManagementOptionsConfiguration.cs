using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace Lombiq.Hosting.Tenants.Maintenance;

public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private const string WwwRoot = "~/" + FeatureNames.Module + "/";
    private const string Js = WwwRoot + "js/";
    private static readonly ResourceManifest _manifest = new();

    static ResourceManagementOptionsConfiguration() =>
        _manifest
            .DefineScript(ResourceNames.StaggeredMaintenance)
            .SetDependencies("bootstrap")
            .SetUrl($"{Js}{ResourceNames.StaggeredMaintenance}.js");

    public void Configure(ResourceManagementOptions options) => options.ResourceManifests.Add(_manifest);
}
