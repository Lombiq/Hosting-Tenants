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
    Task IFeatureEventHandler.InstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

    //async Task IFeatureEventHandler.InstalledAsync(IFeatureInfo feature)
    //{
    //    await EnableAlwaysEnabledFeaturesAsync(feature);
    //    await EnableMediaAndRelatedFeaturesAsync(feature);

    //    // first check if all these features are disabled by default, then see if running the method does properly enable them.
    //    // After that, see what happens when the already enabled ones are removed from recipe
    //        // disabled:
    //        // - Azure Storage
    //        // - Media Cache
    //        // -
    //        // -
    //        // -
    //}

    Task IFeatureEventHandler.EnablingAsync(IFeatureInfo feature) => Task.CompletedTask;

    //Task IFeatureEventHandler.EnabledAsync(IFeatureInfo feature) => EnableMediaAndRelatedFeaturesAsync(feature);
    Task IFeatureEventHandler.EnabledAsync(IFeatureInfo feature) => KeepMediaRelatedDependentFeaturesEnabledAsync(feature);

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

    public async Task EnableMediaAndRelatedFeaturesAsync(IFeatureInfo featureInfo)
    {
        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();

        // make a list of these
        var mediaAndRelatedFeatures = allFeatures.Where(feature => feature.Id is FeatureNames.AzureStorage or FeatureNames.Media or
            FeatureNames.Contents or FeatureNames.ContentTypes or FeatureNames.Liquid or FeatureNames.Settings);
        //var mediaAndRelatedFeatures = allFeatures.Where(feature => feature.Id is FeatureNames.AzureStorage or FeatureNames.Media or
        //    FeatureNames.MediaCache or FeatureNames.Contents or FeatureNames.ContentTypes or FeatureNames.Liquid or FeatureNames.Settings);

        var currentlyEnabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();
        var featuresToEnable = new List<IFeatureInfo>();

        // enable Media first (if not already enabled). Then see if more dependencies need explicit enabling before running the foreach
            // is this even the problem?
        //var mediaFeature = allFeatures.Where(feature => feature.Id == FeatureNames.Media);
        //var isMediaEnabled = currentlyEnabledFeatures.Contains(mediaFeature.First());

        //if (!isMediaEnabled)
        //{
        //    var borkedList = new List<IFeatureInfo> { mediaFeature.FirstOrDefault() };
        //    await _shellFeaturesManager.EnableFeaturesAsync(borkedList);
        //}

        //// this does not update in real time
        //var newlyEnabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();

        foreach (var feature in mediaAndRelatedFeatures)
        {
            if (currentlyEnabledFeatures.Contains(feature))
            {
                continue;
            }

            if (feature.Id != FeatureNames.AzureStorage)
            {
                // just enable here?
                featuresToEnable.Add(feature);
                //var featureToEnable = new List<IFeatureInfo>
                //{
                //    feature,
                //};

                //await _shellFeaturesManager.EnableFeaturesAsync(featureToEnable);
            }
            else
            {
                // Azure Storage can only be enabled once Media Cache is.
                if (_configuration.IsAzureHosting() && featureInfo.Id == FeatureNames.MediaCache) // test this locally too
                {
                    featuresToEnable.Add(feature);

                    //var featureToEnable = new List<IFeatureInfo>
                    //{
                    //    feature,
                    //};

                    //await _shellFeaturesManager.EnableFeaturesAsync(featureToEnable);
                }

                continue;
            }
        }

        if (!featuresToEnable.Any()) return;

        // enabling them in bulk might be the cause of the shell descriptor exception.
        // try enabling one by one
        // either in a foreach in this method
        // or separately on each call of this method

        // Enable the features one by one to avoid Shell Descriptor exception. as fucking if

        // could always return or call this method from inside the method?
        // check for dependencies first, enable those before the main feature?

        // debug and see which feature causes exception -- Media Cache. Of course.
            // but it's not only MC, if other features are not enabled in recipe, multiple features throw this error
        await _shellFeaturesManager.EnableFeaturesAsync(featuresToEnable, force: true);
        //var eke = new List<IFeatureInfo> { featuresToEnable.FirstOrDefault() };
        //if (eke.Any())
        //{
        //    await _shellFeaturesManager.EnableFeaturesAsync(eke, force: true);
        //}

        var ga = "";
        //foreach (var feature in featuresToEnable)
        //{
        //    var eke = new List<IFeatureInfo>
        //    {
        //        feature,
        //    };

        //    await _shellFeaturesManager.EnableFeaturesAsync(eke);
        //}
    }

    // Keeps certain features enabled if they are dependent on other features. Or something.
    public async Task KeepMediaRelatedDependentFeaturesEnabledAsync(IFeatureInfo featureInfo) // EnabledAsync
    {
        if (featureInfo.Id is not FeatureNames.Media and not FeatureNames.MediaCache and not
            FeatureNames.ContentTypes and not FeatureNames.Contents and not FeatureNames.Liquid)
        {
            return;
        }

        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();

        if (featureInfo.Id == FeatureNames.Liquid)
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
            //if (!_configuration.IsAzureHosting()) return;

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
            //if (!_configuration.IsAzureHosting()) return;

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
