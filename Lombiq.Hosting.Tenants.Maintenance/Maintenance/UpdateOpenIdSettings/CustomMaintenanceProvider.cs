using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeFreund.OpenId.Constants;
using OfficeFreund.OpenId.Services;
using OfficeFreund.OpenId.Settings;
using OrchardCore.ContentManagement.Utilities;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.OpenId.Configuration;
using OrchardCore.OpenId.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.UpdateOpenIdSettings;

public class CustomMaintenanceProvider : MaintenanceProviderBase
{
    private readonly ShellSettings _shellSettings;
    private readonly ILogger<CustomMaintenanceProvider> _logger;
    private readonly CustomMaintenanceOptions _options;
    private readonly IEnumerable<ICustomMaintenanceHandler> _customMaintenanceHandlers;

    public CustomMaintenanceProvider(
        ShellSettings shellSettings,
        IOptions<CustomMaintenanceOptions> updateOpenIdSettingsMaintenanceOptions,
        ILogger<CustomMaintenanceProvider> logger,
        IEnumerable<ICustomMaintenanceHandler> customMaintenanceHandlers)
    {
        _shellSettings = shellSettings;
        _options = updateOpenIdSettingsMaintenanceOptions.Value;
        _logger = logger;
        _customMaintenanceHandlers = customMaintenanceHandlers;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(_options.IsEnabled && !context.WasLatestExecutionSuccessful());

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        context.ReloadShellAfterMaintenanceCompletion = true;

        _logger.LogInformation("Custom maintenance was started for {TenantName} tenant.", _shellSettings.Name);

        await _customMaintenanceHandlers.AwaitEachAsync(handler => handler.ExecuteAsync(context));

        _logger.LogInformation("Custom maintenance was done for {TenantName} tenant.", _shellSettings.Name);
    }
}
