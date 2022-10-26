using Lombiq.Hosting.Tenants.FeaturesGuard.Constants;
using Lombiq.Hosting.Tenants.FeaturesGuard.Models;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using System;
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
        await EnableConditionallyEnabledFeaturesAsync(feature);
    }

    Task IFeatureEventHandler.DisablingAsync(IFeatureInfo feature) => Task.CompletedTask;

    async Task IFeatureEventHandler.DisabledAsync(IFeatureInfo feature)
    {
        //await KeepConditionallyEnabledFeaturesEnabledAsync(feature);
        await DisableConditionallyEnabledFeaturesAsync(feature);
    }

    Task IFeatureEventHandler.UninstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.UninstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

    public async Task EnableConditionallyEnabledFeaturesAsync(IFeatureInfo featureInfo) // EnabledAsync
    {
        // CONDITIONAL FEATURE - feature A (key)
        // CONDITION FEATURE - feature B (value)

        if (_shellSettings.IsDefaultShell() ||
            _conditionallyEnabledFeaturesOptions.Value.EnableFeatureIfOtherFeatureIsEnabled is not { } conditionallyEnabledFeatures)
        {
            return;
        }

        var allConditionFeatureIds = new List<string>(); // need this be distinct?
        foreach (var conditionFeatureId in conditionallyEnabledFeatures.Values)
        {
            var separatedConditionFeatureIds = conditionFeatureId.SplitByCommas();
            allConditionFeatureIds.AddRange(separatedConditionFeatureIds.Select(id => id.Trim()));
        }

        if (!allConditionFeatureIds.Contains(featureInfo.Id))
        {
            return;
        }

        // If current feature is one of the condition features, enable its corresponding conditional features if they
        // are not already enabled.
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

        // Handle multiple conditional features as well.
        var currentlyDisabledConditionalFeautures = currentlyDisabledFeatures.Intersect(conditionalFeatures);
        if (currentlyDisabledConditionalFeautures.Any())
        {
            await _shellFeaturesManager.EnableFeaturesAsync(currentlyDisabledConditionalFeautures, force: true);
        }
    }

    public async Task KeepConditionallyEnabledFeaturesEnabledAsync(IFeatureInfo featureInfo) // DisabledAsync
    {
        // CONDITIONAL FEATURE - feature A (key)
        // CONDITION FEATURE - feature B (value)

        if (_shellSettings.IsDefaultShell() ||
            _conditionallyEnabledFeaturesOptions.Value.EnableFeatureIfOtherFeatureIsEnabled is not { } conditionallyEnabledFeatures)
        {
            return;
        }

        if (!conditionallyEnabledFeatures.ContainsKey(featureInfo.Id))
        {
            return;
        }

        // If condition feature is enabled, re-enable conditional feature.
        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();
        var conditionFeatureId = conditionallyEnabledFeatures[featureInfo.Id];
        var conditionFeature = allFeatures.Where(feature => feature.Id == conditionFeatureId);

        var currentlyEnabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();
        if (currentlyEnabledFeatures.Contains(conditionFeature.SingleOrDefault()))
        {
            var conditionalFeature = allFeatures.Where(feature => feature.Id == featureInfo.Id);

            // Don't force as manually disabled dependencies should disable the conditional feature.
            await _shellFeaturesManager.EnableFeaturesAsync(conditionalFeature);
        }
    }

    public async Task DisableConditionallyEnabledFeaturesAsync(IFeatureInfo featureInfo) // DisabledAsync
    {
        // CONDITIONAL FEATURE - feature A (key)
        // CONDITION FEATURE - feature B (value)

        if (_shellSettings.IsDefaultShell() ||
            _conditionallyEnabledFeaturesOptions.Value.EnableFeatureIfOtherFeatureIsEnabled is not { } conditionallyEnabledFeatures)
        {
            return;
        }

        var allConditionFeatureIds = new List<string>(); // need this be distinct?
        foreach (var conditionFeatureId in conditionallyEnabledFeatures.Values)
        {
            var separatedConditionFeatureIds = conditionFeatureId.SplitByCommas();
            allConditionFeatureIds.AddRange(separatedConditionFeatureIds.Select(id => id.Trim()));
        }

        if (!allConditionFeatureIds.Contains(featureInfo.Id))
        {
            return;
        }

        // If current feature is one of the condition features, disable its corresponding conditional features if they
        // are not already disabled.
        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();

        var conditionalFeatureIds = new List<string>();
        var conditionFeatureIds = new List<string>();
        foreach (var keyValuePair in conditionallyEnabledFeatures)
        {
            var valueFormatted = new List<string>();
            var separatedValues = keyValuePair.Value.SplitByCommas();
            foreach (var separatedValue in separatedValues)
            {
                valueFormatted.Add(separatedValue.Trim());
            }

            if (valueFormatted.Contains(featureInfo.Id))
            {
                conditionalFeatureIds.Add(keyValuePair.Key);
                conditionFeatureIds.AddRange(valueFormatted);
            }
        }

        var conditionalFeatures = allFeatures.Where(feature => conditionalFeatureIds.Contains(feature.Id));
        var conditionFeatures = allFeatures.Where(feature => conditionFeatureIds.Contains(feature.Id));
        var currentlyEnabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();

        // Only disable conditional feature if none of its condition features are enabled.
        var currentlyEnabledConditionFeatures = currentlyEnabledFeatures.Intersect(conditionFeatures);

        // Handle multiple conditional features as well.
        var currentlyEnabledConditionalFeautures = currentlyEnabledFeatures.Intersect(conditionalFeatures);
        if (currentlyEnabledConditionalFeautures.Any() && !currentlyEnabledConditionFeatures.Any())
        {
            await _shellFeaturesManager.DisableFeaturesAsync(currentlyEnabledConditionalFeautures);
        }
    }
}
