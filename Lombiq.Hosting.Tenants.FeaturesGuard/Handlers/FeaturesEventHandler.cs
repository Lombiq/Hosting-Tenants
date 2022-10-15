using Lombiq.Hosting.Tenants.FeaturesGuard.Constants;
using Lombiq.Hosting.Tenants.FeaturesGuard.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql.Services;

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

    Task IFeatureEventHandler.InstalledAsync(IFeatureInfo feature) => EnableAlwaysEnabledFeaturesAsync(feature);

    Task IFeatureEventHandler.EnablingAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.EnabledAsync(IFeatureInfo feature) => EnableMediaAndRelatedFeaturesAsync(feature);
    // Task IFeatureEventHandler.EnabledAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.DisablingAsync(IFeatureInfo feature) => Task.CompletedTask;

    async Task IFeatureEventHandler.DisabledAsync(IFeatureInfo feature)
    {
        await KeepAlwaysEnabledFeaturesEnabledAsync(feature);
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

        // when should this be run though -- is it not a problem if it runs immediately after the very first feature is enabled?
            // sounds like a problem, but it seemed fine when actually running it?
        // go by feature priority or category?
        // try running this after a feature that has a priority greater than 0 was enabled -- but then again, having a 0 < priority feature is not guaranteed

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

    public async Task EnableMediaAndRelatedFeaturesAsync(IFeatureInfo featureInfo)
    {
        // Enable Media if it is not already enabled.
        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();
        var mediaAndRelatedFeatures = allFeatures.Where(feature => feature.Id is FeatureNames.AzureStorage or FeatureNames.Media or
            FeatureNames.MediaCache or FeatureNames.Contents or FeatureNames.ContentTypes or FeatureNames.Liquid or FeatureNames.Settings);

        var currentlyEnabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();
        var featuresToEnable = new List<IFeatureInfo>();
        foreach (var feature in mediaAndRelatedFeatures)
        {
            if (!currentlyEnabledFeatures.Contains(feature))
            {
                if (feature.Id == FeatureNames.AzureStorage && _configuration.IsAzureHosting())
                {
                    featuresToEnable.Add(feature);

                    continue;
                }

                featuresToEnable.Add(feature);
            }
        }

        if (!featuresToEnable.Any()) return;

        await _shellFeaturesManager.EnableFeaturesAsync(featuresToEnable);
    }

    public async Task KeepMediaAndRelatedFeaturesEnabledAsync(IFeatureInfo featureInfo)
    {
        if (featureInfo.Id is not FeatureNames.AzureStorage and not FeatureNames.Media and not FeatureNames.MediaCache and not
            FeatureNames.Contents and not FeatureNames.ContentTypes and not FeatureNames.Liquid and not FeatureNames.Settings)
        {
            return;
        }

        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();
        if (featureInfo.Id != FeatureNames.AzureStorage)
        {
            // enable the feature
            var featureToKeepEnabled = allFeatures.Where(feature => feature.Id == featureInfo.Id);
            await _shellFeaturesManager.EnableFeaturesAsync(featureToKeepEnabled);
        }
        else
        {
            if (!_configuration.IsAzureHosting()) return;

            var azureMedia = allFeatures.Where(feature => feature.Id == FeatureNames.AzureStorage);
            await _shellFeaturesManager.EnableFeaturesAsync(azureMedia);
        }
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
