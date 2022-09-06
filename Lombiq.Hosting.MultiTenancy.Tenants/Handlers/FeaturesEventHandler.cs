using Lombiq.Hosting.MultiTenancy.Tenants.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Handlers;

public sealed class FeaturesEventHandler : IFeatureEventHandler
{
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly IOptions<AlwaysEnabledFeaturesOptions> _alwaysEnabledFeaturesOptions;
    private readonly ShellSettings _shellSettings;
    private readonly IHttpContextAccessor _hca;

    public FeaturesEventHandler(
        IShellFeaturesManager shellFeaturesManager,
        IOptions<AlwaysEnabledFeaturesOptions> alwaysEnabledFeaturesOptions,
        ShellSettings shellSettings,
        IHttpContextAccessor hca)
    {
        _shellFeaturesManager = shellFeaturesManager;
        _alwaysEnabledFeaturesOptions = alwaysEnabledFeaturesOptions;
        _shellSettings = shellSettings;
        _hca = hca;
    }

    Task IFeatureEventHandler.InstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.InstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.EnablingAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.EnabledAsync(IFeatureInfo feature) => EnableMediaRelatedFeaturesAsync(feature);

    Task IFeatureEventHandler.DisablingAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.DisabledAsync(IFeatureInfo feature) => KeepFeaturesEnabledAsync(feature);

    Task IFeatureEventHandler.UninstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.UninstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

    public async Task EnableMediaRelatedFeaturesAsync(IFeatureInfo feature)
    {
        if (feature.Id is not "OrchardCore.Media" and not "OrchardCore.Media.Cache") return; // add constants for feature names

        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();

        if (feature.Id == "OrchardCore.Media")
        {
            var featuresToEnable = allFeatures.Where(feature => feature.Id is "OrchardCore.ContentTypes" or
                "OrchardCore.Liquid" or "OrchardCore.Media.Cache" or "OrchardCore.Settings");

            await _shellFeaturesManager.EnableFeaturesAsync(featuresToEnable);
        }
        else
        {
            var azureMedia = allFeatures.Where(feature => feature.Id is "OrchardCore.Media.Azure.Storage");
            await _shellFeaturesManager.EnableFeaturesAsync(azureMedia);
        }
    }

    public async Task KeepFeaturesEnabledAsync(IFeatureInfo feature)
    {
        if (_shellSettings.IsDefaultShell() || !_alwaysEnabledFeaturesOptions.Value.AlwaysEnabledFeatures.Contains(feature.Id))
        {
            return;
        }

        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();
        var currentFeature = allFeatures.Where(f => f.Id == feature.Id);

        await _shellFeaturesManager.EnableFeaturesAsync(currentFeature);
    }
}
