using Lombiq.Hosting.Tenants.FeaturesGuard.Constants;
using Lombiq.Hosting.Tenants.FeaturesGuard.Models;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.FeaturesGuard.Handlers;

public sealed class FeaturesEventHandler : IFeatureEventHandler
{
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly IOptions<ConditionallyEnabledFeaturesOptions> _conditionallyEnabledFeaturesOptions;
    private readonly ShellSettings _shellSettings;
    private readonly IConfiguration _configuration;
    private readonly IExtensionManager _extensionManager;

    public FeaturesEventHandler(
        IShellFeaturesManager shellFeaturesManager,
        IOptions<ConditionallyEnabledFeaturesOptions> conditionallyEnabledFeaturesOptions,
        ShellSettings shellSettings,
        IConfiguration configuration,
        IExtensionManager extensionManager)
    {
        _shellFeaturesManager = shellFeaturesManager;
        _conditionallyEnabledFeaturesOptions = conditionallyEnabledFeaturesOptions;
        _shellSettings = shellSettings;
        _configuration = configuration;
        _extensionManager = extensionManager;
    }

    Task IFeatureEventHandler.InstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.InstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.EnablingAsync(IFeatureInfo feature) => Task.CompletedTask;

    async Task IFeatureEventHandler.EnabledAsync(IFeatureInfo feature)
    {
        await EnableConditionallyEnabledFeatureAsync(feature);
    }

    Task IFeatureEventHandler.DisablingAsync(IFeatureInfo feature) => Task.CompletedTask;

    async Task IFeatureEventHandler.DisabledAsync(IFeatureInfo feature)
    {
    }

    Task IFeatureEventHandler.UninstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.UninstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

    // Keeps certain features enabled if they are dependent on other features. Or something.
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
    // not even necessary? I am in shambles
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

    public async Task EnableConditionallyEnabledFeatureAsync(IFeatureInfo featureInfo) // EnabledAsync
    {
        // CONDITIONAL FEATURE - feature A (key)
        // CONDITION FEATURE - feature B (value)

        if (_shellSettings.IsDefaultShell() ||
            _conditionallyEnabledFeaturesOptions.Value.EnableFeatureIfOtherFeatureIsEnabled is not { } conditionallyEnabledFeatures)
        {
            return;
        }

        if (!conditionallyEnabledFeatures.Values.Contains(featureInfo.Id))
        {
            return;
        }

        // so just... if featureInfo.Id is one of the condition features, enable its corresponding conditional feature
            // unless the conditional feature is already enabled

        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();

        var conditionalFeatureIds = new List<string>();
        foreach (var keyValuePair in conditionallyEnabledFeatures)
        {
            if (keyValuePair.Value == featureInfo.Id)
            {
                conditionalFeatureIds.Add(keyValuePair.Key);
            }
        }

        var conditionalFeatures = allFeatures.Where(feature => conditionalFeatureIds.Contains(feature.Id));

        var currentlyEnabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();
        var currentlyDisabledFeatures = allFeatures.Except(currentlyEnabledFeatures);

        // account for multiple
        var currentlyDisabledConditionalFeautures = currentlyDisabledFeatures.Intersect(conditionalFeatures);
        if (currentlyDisabledConditionalFeautures.Any())
        {
            await _shellFeaturesManager.EnableFeaturesAsync(currentlyDisabledConditionalFeautures, force: true);
        }
    }

    public async Task EnableAlwaysEnabledFeaturesAsync(IFeatureInfo featureInfo) // EnabledAsync
    {
        if (_shellSettings.IsDefaultShell() ||
            _conditionallyEnabledFeaturesOptions.Value.EnableFeatureIfOtherFeatureIsEnabled is not { } conditionallyEnabledFeatures)
        {
            return;
        }

        // This method also runs on setup -- perhaps it can be allowed to run only after OC.Settings is enabled?
        // any downsides to letting it run as it pleases? Check log on new setup


        // this is also run after a dependency was just re-enabled. Get the dependent feature by looking at the dependencies of
        // the always enabled features, then enable the feature that is dependent on the featureInfo.Id feature
        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();
        //var alwaysEnabledFeaturesInfo = allFeatures.Where(feature => alwaysEnabledFeatures.Contains(feature.Id));
        var currentlyEnabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();

        // Find all dependent features.
        //var allDependentFeatures = alwaysEnabledFeaturesInfo.Where(feature => feature.Dependencies.Any());

        // Find all dependencies.
        var allDependencies = new List<IFeatureInfo>();
        //foreach (var alwaysEnabledFeature in alwaysEnabledFeaturesInfo)
        //{
        //    if (alwaysEnabledFeature.Dependencies.Any())
        //    {
        //        foreach (var dependency in alwaysEnabledFeature.Dependencies)
        //        {
        //            allDependencies.AddRange(allFeatures.Where(feature => feature.Id == dependency));
        //        }
        //    }
        //}

        var enabledDependencies = currentlyEnabledFeatures.Intersect(allDependencies);

        var dependentFeaturesThatCanBeEnabled = new List<IFeatureInfo>();
        //if (enabledDependencies.Any())
        //{
        //    foreach (var dependentFeature in allDependentFeatures)
        //    {
        //        foreach (var enabledDependency in enabledDependencies)
        //        {
        //            if (dependentFeature.Dependencies.Contains(enabledDependency.Id))
        //            {
        //                dependentFeaturesThatCanBeEnabled.Add(dependentFeature);
        //            }
        //        }
        //    }
        //}

        // don't we need to enable dependencies first?
        // find dependencies that need enabling
        var currentlyDisabledDependencies = allDependencies.Where(dependency => !currentlyEnabledFeatures.Contains(dependency));
        if (currentlyDisabledDependencies.Any())
        {
            await _shellFeaturesManager.EnableFeaturesAsync(currentlyDisabledDependencies);

            return;
        }

        // run this if no dependencies need enabling
        // Find features that can be enabled and are not yet enabled.
        var featuresToEnable = dependentFeaturesThatCanBeEnabled
            .Distinct()
            .Where(feature => !currentlyEnabledFeatures.Contains(feature));

        if (featuresToEnable.Any())
        {
            var featureToEnableList = new List<IFeatureInfo> { featuresToEnable.FirstOrDefault() };
            await _shellFeaturesManager.EnableFeaturesAsync(featureToEnableList);
        }


        // Can likely be run after OC.Settings is enabled as it's set to AlwaysEnabled.
            // but features cannot be enabled in bulk, so how is it going to be done one by one?
                // could just go by whether featureInfo.Id is in alwaysEnabledFeatures


        //var featuresToEnable = new List<IFeatureInfo>();
        //foreach (var feature in alwaysEnabledFeaturesInfo)
        //{
        //    if (!currentlyEnabledFeatures.Contains(feature))
        //    {
        //        featuresToEnable.Add(feature);
        //    }
        //}

        //if (!featuresToEnable.Any()) return;

        //await _shellFeaturesManager.EnableFeaturesAsync(featuresToEnable);
    }

    public async Task KeepAlwaysEnabledFeaturesEnabledAsync(IFeatureInfo featureInfo) // DisabledAsync
    {
        //if (_shellSettings.IsDefaultShell() ||
        //    _alwaysEnabledFeaturesOptions.Value.AlwaysEnabledFeatures is not { } alwaysEnabledFeatures ||
        //    !alwaysEnabledFeatures.Contains(featureInfo.Id))
        //{
        //    return;
        //}

        //if (!_alwaysEnabledFeaturesOptions.Value.AlwaysEnabledFeatures.Contains(featureInfo.Id))
        //{
        //    return;
        //}



        // Need to deal with dependencies first -- how
        // require specifying dependencies for each feature in appsettings.json?
        // a dynamic solution would be better and could be used for Media and co as well -- check in OC source if this is available
        // can anything be done with featureInfo's [] Dependencies?

        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();
        var currentlyEnabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();

        if (featureInfo.Dependencies.Any())
        {
            var currentlyDisabledDependencies = new List<IFeatureInfo>();

            // If a dependency is not enabled, enable it first.
            // enabling them in a loop seems to cause exception. Enable only one, and do the rest in EnabledAsync()?
            
            
            
            //var dependencyFeature = allFeatures.Where(feature => feature.Id == featureInfo.Dependencies.FirstOrDefault());
            //var isDependencyFeatureEnabled = currentlyEnabledFeatures.Contains(dependencyFeature.SingleOrDefault());
            //if (!isDependencyFeatureEnabled)
            //{
            //    currentlyDisabledDependencies.AddRange(dependencyFeature);
            //    await _shellFeaturesManager.EnableFeaturesAsync(dependencyFeature);
            //}


            foreach (var dependency in featureInfo.Dependencies)
            {
                var dependencyFeature = allFeatures.Where(feature => feature.Id == dependency);

                var isDependencyFeatureEnabled = currentlyEnabledFeatures.Contains(dependencyFeature.SingleOrDefault());
                if (!isDependencyFeatureEnabled)
                {
                    currentlyDisabledDependencies.AddRange(dependencyFeature);
                }
            }

            // Cannot enable dependent feature here if any of the dependencies are only just being enabled.
            if (currentlyDisabledDependencies.Any())
            {
                // does not work if there are multiple dependencies -- it will try to enable the same dependency multiple times
                // Cannot enable all dependencies at once, so enable one and take care of the rest in EnabledAsync().
                //var dependencyToEnable = new List<IFeatureInfo> { currentlyDisabledDependencies.FirstOrDefault() };
                //await _shellFeaturesManager.EnableFeaturesAsync(dependencyToEnable);

                // fuck it, enable OC.Settings and let EnabledAsync() do the rest
                var settingsFeature = allFeatures.Where(feature => feature.Id == FeatureNames.Settings);
                await _shellFeaturesManager.EnableFeaturesAsync(settingsFeature);

                return;
            }
        }

        var currentFeature = allFeatures.Where(feature => feature.Id == featureInfo.Id);
        await _shellFeaturesManager.EnableFeaturesAsync(currentFeature);
    }
}
