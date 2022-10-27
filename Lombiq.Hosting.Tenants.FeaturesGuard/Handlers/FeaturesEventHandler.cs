using Lombiq.Hosting.Tenants.FeaturesGuard.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OrchardCore.DisplayManagement.Shapes.ShapeDebugView;

namespace Lombiq.Hosting.Tenants.FeaturesGuard.Handlers;

public sealed class FeaturesEventHandler : IFeatureEventHandler
{
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly IOptions<ConditionallyEnabledFeaturesOptions> _conditionallyEnabledFeaturesOptions;
    private readonly ShellSettings _shellSettings;

    public FeaturesEventHandler(
        IShellFeaturesManager shellFeaturesManager,
        IOptions<ConditionallyEnabledFeaturesOptions> conditionallyEnabledFeaturesOptions,
        ShellSettings shellSettings,
        IConfiguration configuration)
    {
        _shellFeaturesManager = shellFeaturesManager;
        _conditionallyEnabledFeaturesOptions = conditionallyEnabledFeaturesOptions;
        _shellSettings = shellSettings;
    }

    Task IFeatureEventHandler.InstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.InstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.EnablingAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.EnabledAsync(IFeatureInfo feature) => EnableConditionallyEnabledFeaturesAsync(feature);

    Task IFeatureEventHandler.DisablingAsync(IFeatureInfo feature) => Task.CompletedTask;

    async Task IFeatureEventHandler.DisabledAsync(IFeatureInfo feature)
    {
        await KeepConditionallyEnabledFeaturesEnabledAsync(feature);
        await DisableConditionallyEnabledFeaturesAsync(feature);
    }

    Task IFeatureEventHandler.UninstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

    Task IFeatureEventHandler.UninstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

    /// <summary>
    /// Enables conditional features (key) if one of their corresponding condition features (value) was enabled.
    /// </summary>
    /// <param name="featureInfo">The feature that was just enabled.</param>
    public async Task EnableConditionallyEnabledFeaturesAsync(IFeatureInfo featureInfo)
    {
        if (_shellSettings.IsDefaultShell() ||
            _conditionallyEnabledFeaturesOptions.Value.EnableFeatureIfOtherFeatureIsEnabled is not { } conditionallyEnabledFeatures)
        {
            return;
        }

        var allConditionFeatureIds = new List<string>();
        foreach (var conditionFeatureId in conditionallyEnabledFeatures.Values)
        {
            var separatedConditionFeatureIds = conditionFeatureId.SplitByCommas();
            allConditionFeatureIds.AddRange(separatedConditionFeatureIds.Select(id => id.Trim()));
        }

        if (!allConditionFeatureIds.Contains(featureInfo.Id))
        {
            return;
        }

        // Enable conditional features if they are not already enabled.
        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();

        var conditionalFeatureIds = new List<string>();
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
            }
        }

        var conditionalFeatures = allFeatures.Where(feature => conditionalFeatureIds.Contains(feature.Id));
        var currentlyEnabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();
        var currentlyDisabledFeatures = allFeatures.Except(currentlyEnabledFeatures);

        // Handle multiple conditional features as well.
        var currentlyDisabledConditionalFeatures = currentlyDisabledFeatures.Intersect(conditionalFeatures);
        if (currentlyDisabledConditionalFeatures.Any())
        {
            await _shellFeaturesManager.EnableFeaturesAsync(currentlyDisabledConditionalFeatures, force: true);
        }
    }

    /// <summary>
    /// When a conditional feature (key) is disabled, keeps the conditional feature enabled if any of the corresponding
    /// condition features (value) are enabled.
    /// </summary>
    /// <param name="featureInfo">The feature that was just disabled.</param>
    public async Task KeepConditionallyEnabledFeaturesEnabledAsync(IFeatureInfo featureInfo)
    {
        if (_shellSettings.IsDefaultShell() ||
            _conditionallyEnabledFeaturesOptions.Value.EnableFeatureIfOtherFeatureIsEnabled is not { } conditionallyEnabledFeatures)
        {
            return;
        }

        if (!conditionallyEnabledFeatures.ContainsKey(featureInfo.Id))
        {
            return;
        }

        // Re-enable conditional feature if any its condition features are enabled.
        var allFeatures = await _shellFeaturesManager.GetAvailableFeaturesAsync();
        var conditionFeatureIds = conditionallyEnabledFeatures[featureInfo.Id];

        var conditionFeatureIdsList = new List<string>();
        var separatedValues = conditionFeatureIds.SplitByCommas();
        foreach (var separatedValue in separatedValues)
        {
            conditionFeatureIdsList.Add(separatedValue.Trim());
        }

        var currentlyEnabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();
        var conditionFeatures = allFeatures.Where(feature => conditionFeatureIdsList.Contains(feature.Id));

        var currentlyEnabledConditionFeatures = currentlyEnabledFeatures.Intersect(conditionFeatures);
        if (currentlyEnabledConditionFeatures.Any())
        {
            var conditionalFeature = allFeatures.Where(feature => feature.Id == featureInfo.Id);

            // Don't force since manually disabled dependencies should disable the conditional feature.
            await _shellFeaturesManager.EnableFeaturesAsync(conditionalFeature);
        }
    }

    /// <summary>
    /// When a condition feature (value) is disabled, disables the corresponding conditional features (key) if all of
    /// their condition features are disabled.
    /// </summary>
    /// <param name="featureInfo">The feature that was just disabled.</param>
    public async Task DisableConditionallyEnabledFeaturesAsync(IFeatureInfo featureInfo)
    {
        if (_shellSettings.IsDefaultShell() ||
            _conditionallyEnabledFeaturesOptions.Value.EnableFeatureIfOtherFeatureIsEnabled is not { } conditionallyEnabledFeatures)
        {
            return;
        }

        var allConditionFeatureIds = new List<string>();
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
        var currentlyEnabledConditionalFeatures = currentlyEnabledFeatures.Intersect(conditionalFeatures);
        if (currentlyEnabledConditionalFeatures.Any() && !currentlyEnabledConditionFeatures.Any())
        {
            await _shellFeaturesManager.DisableFeaturesAsync(currentlyEnabledConditionalFeatures);
        }
    }
}
