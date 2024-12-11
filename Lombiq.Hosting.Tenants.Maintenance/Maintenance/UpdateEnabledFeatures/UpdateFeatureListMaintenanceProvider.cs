using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateEnabledFeatures;

public class UpdateEnabledFeaturesMaintenanceProvider : MaintenanceProviderBase
{
    private readonly UpdateEnabledFeaturesMaintenanceOptions _options;
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly ILogger<UpdateEnabledFeaturesMaintenanceProvider> _logger;

    public UpdateEnabledFeaturesMaintenanceProvider(
        IOptions<UpdateEnabledFeaturesMaintenanceOptions> options,
        IShellFeaturesManager shellFeaturesManager,
        ILogger<UpdateEnabledFeaturesMaintenanceProvider> logger)
    {
        _options = options.Value;
        _shellFeaturesManager = shellFeaturesManager;
        _logger = logger;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(_options.IsEnabled && !context.WasLatestExecutionSuccessful());

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var availableFeatures = (await _shellFeaturesManager.GetAvailableFeaturesAsync()).ToList();

        if (ReplaceAndSplitByCommas(_options.EnableFeatures) is { } enableFeaturesOption)
        {
            var enableFeatures = availableFeatures.Where(feature =>
                enableFeaturesOption.Contains(feature.Id)).ToList();
            await _shellFeaturesManager.EnableFeaturesAsync(enableFeatures, force: true);
            await NotifyAsync(enableFeatures);
        }

        if (ReplaceAndSplitByCommas(_options.DisableFeatures) is { } disableFeaturesOption)
        {
            var disableFeatures = availableFeatures.Where(feature =>
                disableFeaturesOption.Contains(feature.Id)).ToList();
            await _shellFeaturesManager.DisableFeaturesAsync(disableFeatures, force: true);
            await NotifyAsync(disableFeatures, enabled: false);
        }
    }

    private ValueTask NotifyAsync(IEnumerable<IFeatureInfo> features, bool enabled = true)
    {
        foreach (var feature in features)
        {
            _logger.LogInformation("{FeatureName} was {Action} by maintenance.", feature.Name ?? feature.Id, enabled ? "enabled" : "disabled");
        }

        return ValueTask.CompletedTask;
    }

    private static string[] ReplaceAndSplitByCommas(string input) =>
        input?.Replace(" ", string.Empty).SplitByCommas();
}
