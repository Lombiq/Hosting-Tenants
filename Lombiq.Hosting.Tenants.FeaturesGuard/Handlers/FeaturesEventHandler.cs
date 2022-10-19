using Lombiq.Hosting.Tenants.FeaturesGuard.Constants;
using Lombiq.Hosting.Tenants.FeaturesGuard.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.FeaturesGuard.Handlers;

public sealed class FeaturesEventHandler : IFeatureEventHandler
{
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly IOptions<AlwaysEnabledFeaturesOptions> _alwaysEnabledFeaturesOptions;
    private readonly ShellSettings _shellSettings;
    private readonly IConfiguration _configuration;

    public FeaturesEventHandler(
        IShellFeaturesManager shellFeaturesManager,
        IOptions<AlwaysEnabledFeaturesOptions> alwaysEnabledFeaturesOptions,
        ShellSettings shellSettings,
        IConfiguration configuration)
    {
        _shellFeaturesManager = shellFeaturesManager;
        _alwaysEnabledFeaturesOptions = alwaysEnabledFeaturesOptions;
        _shellSettings = shellSettings;
        _configuration = configuration;
    }

    Task IFeatureEventHandler.InstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.InstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.EnablingAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.EnabledAsync(IFeatureInfo feature) => EnableAndKeepMediaRelatedDependentFeaturesEnabledAsync(feature);

    Task IFeatureEventHandler.DisablingAsync(IFeatureInfo feature) => Task.CompletedTask;

    async Task IFeatureEventHandler.DisabledAsync(IFeatureInfo feature)
    {
        //await KeepAlwaysEnabledFeaturesEnabledAsync(feature);
        await KeepMediaAndRelatedFeaturesEnabledAsync(feature);
    }

    Task IFeatureEventHandler.UninstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.UninstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

    public async Task EnableAlwaysEnabledFeaturesAsync(IFeatureInfo featureInfo)
    {
        if (_shellSettings.IsDefaultShell() ||
            _alwaysEnabledFeaturesOptions.Value.AlwaysEnabledFeatures is not { } alwaysEnabledFeatures)
        {
            return;
        }

        // Can likely be run after OC.Settings is enabled as it's set to AlwaysEnabled.

        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();
        var alwaysEnabledFeaturesInfo = allFeatures.Where(feature => alwaysEnabledFeatures.Contains(feature.Id));
        var currentlyEnabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();

        var featuresToEnable = new List<IFeatureInfo>();
        foreach (var feature in alwaysEnabledFeaturesInfo)
        {
            if (!currentlyEnabledFeatures.Contains(feature))
            {
                featuresToEnable.Add(feature);
            }
        }

        if (!featuresToEnable.Any()) return;

        await _shellFeaturesManager.EnableFeaturesAsync(featuresToEnable);
    }

    // Keeps certain features enabled if they are dependent on other features. Or something.
    // Doesn't this also work during setup?
    public async Task EnableAndKeepMediaRelatedDependentFeaturesEnabledAsync(IFeatureInfo featureInfo) // EnabledAsync
    {
        if (featureInfo.Id is not FeatureNames.Media and not FeatureNames.MediaCache and not
            FeatureNames.ContentTypes and not FeatureNames.Contents and not FeatureNames.Liquid and not FeatureNames.Settings)
        {
            return;
        }

        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();

        // Settings is always enabled, so the process can start with it.
        if (featureInfo.Id == FeatureNames.Settings)
        {
            var liquidFeature = allFeatures.Where(feature => feature.Id == FeatureNames.Liquid);
            await _shellFeaturesManager.EnableFeaturesAsync(liquidFeature);
        }
        else if (featureInfo.Id == FeatureNames.Liquid)
        {
            var contentsFeature = allFeatures.Where(feature => feature.Id == FeatureNames.Contents);
            await _shellFeaturesManager.EnableFeaturesAsync(contentsFeature);
        }
        else if (featureInfo.Id == FeatureNames.Contents)
        {
            var contentTypesFeature = allFeatures.Where(feature => feature.Id == FeatureNames.ContentTypes);
            await _shellFeaturesManager.EnableFeaturesAsync(contentTypesFeature);
        }
        else if (featureInfo.Id == FeatureNames.ContentTypes)
        {
            var mediaFeature = allFeatures.Where(feature => feature.Id == FeatureNames.Media);
            await _shellFeaturesManager.EnableFeaturesAsync(mediaFeature);
        }
        else if (featureInfo.Id == FeatureNames.Media)
        {
            var mediaCacheFeature = allFeatures.Where(feature => feature.Id == FeatureNames.MediaCache);
            await _shellFeaturesManager.EnableFeaturesAsync(mediaCacheFeature);
        }
        else if (featureInfo.Id == FeatureNames.MediaCache)
        {
            if (!_configuration.IsAzureHosting()) return;

            var azureMediaFeature = allFeatures.Where(feature => feature.Id == FeatureNames.AzureStorage);
            await _shellFeaturesManager.EnableFeaturesAsync(azureMediaFeature);
        }
    }

    // Keeps certain features enabled if they don't have any dependent features.
    public async Task KeepMediaAndRelatedFeaturesEnabledAsync(IFeatureInfo featureInfo) // DisabledAsync
    {
        if (featureInfo.Id is not FeatureNames.AzureStorage and not FeatureNames.Media and not FeatureNames.MediaCache and not
            FeatureNames.Contents and not FeatureNames.ContentTypes and not FeatureNames.Liquid)
        {
            return;
        }

        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();
        var currentlyEnabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();

        if (featureInfo.Id == FeatureNames.Contents)
        {
            // Re-enable Contents if Liquid is enabled.
            var liquidFeature = allFeatures.Where(feature => feature.Id == FeatureNames.Liquid);
            if (currentlyEnabledFeatures.Contains(liquidFeature.SingleOrDefault()))
            {
                var contentsFeature = allFeatures.Where(feature => feature.Id == FeatureNames.Contents);
                await _shellFeaturesManager.EnableFeaturesAsync(contentsFeature);
            }

            return;
        }
        else if (featureInfo.Id == FeatureNames.ContentTypes)
        {
            // Re-enable Content Types if Contents is enabled.
            var contentsFeature = allFeatures.Where(feature => feature.Id == FeatureNames.Contents);
            if (currentlyEnabledFeatures.Contains(contentsFeature.SingleOrDefault()))
            {
                var contentTypesFeature = allFeatures.Where(feature => feature.Id == FeatureNames.ContentTypes);
                await _shellFeaturesManager.EnableFeaturesAsync(contentTypesFeature);
            }

            return;
        }
        else if (featureInfo.Id == FeatureNames.Media)
        {
            // Re-enable Media if Content Types is enabled.
            var contentTypesFeature = allFeatures.Where(feature => feature.Id == FeatureNames.ContentTypes);
            if (currentlyEnabledFeatures.Contains(contentTypesFeature.SingleOrDefault()))
            {
                var mediaFeature = allFeatures.Where(feature => feature.Id == FeatureNames.Media);
                await _shellFeaturesManager.EnableFeaturesAsync(mediaFeature);
            }

            return;
        }
        else if (featureInfo.Id == FeatureNames.MediaCache)
        {
            // Re-enable Media Cache if Media is enabled.
            var mediaFeature = allFeatures.Where(feature => feature.Id == FeatureNames.Media);
            if (currentlyEnabledFeatures.Contains(mediaFeature.SingleOrDefault()))
            {
                var mediaCacheFeature = allFeatures.Where(feature => feature.Id == FeatureNames.MediaCache);
                await _shellFeaturesManager.EnableFeaturesAsync(mediaCacheFeature);
            }

            return;
        }
        else if (featureInfo.Id == FeatureNames.AzureStorage)
        {
            if (!_configuration.IsAzureHosting()) return;

            // Re-enable Azure Storage if Media Cache is enabled.
            var mediaCacheFeature = allFeatures.Where(feature => feature.Id == FeatureNames.MediaCache);
            if (currentlyEnabledFeatures.Contains(mediaCacheFeature.SingleOrDefault()))
            {
                var azureMedia = allFeatures.Where(feature => feature.Id == FeatureNames.AzureStorage);
                await _shellFeaturesManager.EnableFeaturesAsync(azureMedia);
            }

            return;
        }

        // Re-enable the feature if it is not a dependent feature.
        var featureToKeepEnabled = allFeatures.Where(feature => feature.Id == featureInfo.Id);
        await _shellFeaturesManager.EnableFeaturesAsync(featureToKeepEnabled, force: true);
    }

    public async Task KeepAlwaysEnabledFeaturesEnabledAsync(IFeatureInfo featureInfo)
    {
        if (_shellSettings.IsDefaultShell() ||
            _alwaysEnabledFeaturesOptions.Value.AlwaysEnabledFeatures is not { } alwaysEnabledFeatures ||
            !alwaysEnabledFeatures.Contains(featureInfo.Id))
        {
            return;
        }

        if (!_alwaysEnabledFeaturesOptions.Value.AlwaysEnabledFeatures.Contains(featureInfo.Id))
        {
            return;
        }

        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();
        var currentFeature = allFeatures.Where(feature => feature.Id == featureInfo.Id);

        await _shellFeaturesManager.EnableFeaturesAsync(currentFeature);
    }
}
