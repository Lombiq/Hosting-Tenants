using Lombiq.Hosting.Tenants.FeaturesGuard.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.FeaturesGuard.Handlers;

public sealed class FeaturesEventHandler : IFeatureEventHandler
{
    private bool _deferredTaskExecuted;

    public Task InstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

    public Task InstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

    public Task EnablingAsync(IFeatureInfo feature) => Task.CompletedTask;

    public Task EnabledAsync(IFeatureInfo feature) => HandleConditionallyEnabledFeaturesAsync();

    public Task DisablingAsync(IFeatureInfo feature) => Task.CompletedTask;

    public Task DisabledAsync(IFeatureInfo feature) => HandleConditionallyEnabledFeaturesAsync();

    public Task UninstallingAsync(IFeatureInfo feature) => Task.CompletedTask; // #spell-check-ignore-line

    public Task UninstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

    /// <summary>
    /// Enables or disables conditional features depending on ConditionallyEnabledFeaturesOptions.
    /// Prevents disabling features that should be enabled according to their conditions.
    /// </summary>
    private Task HandleConditionallyEnabledFeaturesAsync()
    {
        if (_deferredTaskExecuted)
        {
            return Task.CompletedTask;
        }

        _deferredTaskExecuted = true;

        ShellScope.AddDeferredTask(async scope =>
        {
            if (scope.ShellContext.Settings.IsDefaultShell())
            {
                return;
            }

            var shellFeaturesManager = scope
                .ServiceProvider
                .GetRequiredService<IShellFeaturesManager>();

            var conditionallyEnabledFeaturesOptions = scope
                .ServiceProvider
                .GetRequiredService<IOptions<ConditionallyEnabledFeaturesOptions>>()
                .Value
                .EnableFeatureIfOtherFeatureIsEnabled;

            var enabledFeatures = (await shellFeaturesManager.GetEnabledFeaturesAsync())
                .ToHashSet();

            var enabledFeaturesIds = enabledFeatures
                .Select(feature => feature.Id)
                .ToHashSet();

            if (!TryGetFeaturesToBeEnabledAndDisabled(
                    conditionallyEnabledFeaturesOptions,
                    enabledFeaturesIds,
                    out var featuresToEnableIds,
                    out var featuresToDisableIds))
            {
                return;
            }

            var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();

            var featuresToEnable = availableFeatures
                .Where(feature => featuresToEnableIds.Contains(feature.Id))
                .ToList();

            if (featuresToEnable.Exists(feature => feature.DefaultTenantOnly || feature.EnabledByDependencyOnly))
            {
                throw new InvalidOperationException("'DefaultTenantOnly' feature can't be enabled by FeaturesGuard.");
            }

            var featuresToDisable = enabledFeatures
                .Where(feature => featuresToDisableIds.Contains(feature.Id))
                .ToList();

            if (featuresToDisable.Exists(feature => feature.IsAlwaysEnabled || feature.EnabledByDependencyOnly))
            {
                throw new InvalidOperationException("'IsAlwaysEnabled' feature can't be disabled by FeaturesGuard.");
            }

            if (!featuresToEnable.Any() && !featuresToDisable.Any())
            {
                return;
            }

            await shellFeaturesManager.UpdateFeaturesAsync(featuresToDisable, featuresToEnable, force: true);
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// Extracts the feature ids from ConditionallyEnabledFeaturesOptions and separates them into
    /// <paramref name="featuresToEnable"></paramref> and  <paramref name="featuresToDisable"></paramref> hash sets
    /// and compares them to <paramref name="enabledFeatureIds"/> collection to determine which features need to be
    /// enabled or disabled.
    /// </summary>
    /// <returns>A boolean value whether ConditionallyEnabledFeaturesOptions is populated or not.
    /// Also produces <paramref name="featuresToEnable"/> and <paramref name="featuresToDisable"/>.
    /// </returns>
    private static bool TryGetFeaturesToBeEnabledAndDisabled(
        IDictionary<string, IEnumerable<string>> conditionallyEnabledFeatures,
        IReadOnlySet<string> enabledFeatureIds,
        out HashSet<string> featuresToEnable,
        out HashSet<string> featuresToDisable)
    {
        if (!conditionallyEnabledFeatures.Any())
        {
            featuresToEnable = null;
            featuresToDisable = null;
            return false;
        }

        var featuresToEnableIds = new HashSet<string>();
        var featuresToDisableIds = new HashSet<string>();

        foreach (var condition in conditionallyEnabledFeatures)
        {
            var hasConditional = enabledFeatureIds.Contains(condition.Key);
            var hasCondition = enabledFeatureIds.Intersect(condition.Value).Any();

            if (hasCondition && !hasConditional)
            {
                featuresToEnableIds.Add(condition.Key);
            }

            if (!hasCondition && hasConditional)
            {
                featuresToDisableIds.Add(condition.Key);
            }
        }

        featuresToEnable = featuresToEnableIds;
        featuresToDisable = featuresToDisableIds;

        return featuresToEnableIds.Any() || featuresToDisableIds.Any();
    }
}
