using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Users.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ChangeUserSensitiveContent;

public class ChangeUserSensitiveContentMaintenanceProvider : MaintenanceProviderBase
{
    public const string ProviderId = "ChangeUserSensitiveContent";

    private readonly IOptions<ChangeUserSensitiveContentMaintenanceOptions> _options;
    private readonly ISession _session;
    private readonly ShellSettings _shellSettings;
    private readonly IChangeUserSensitiveContentQueue _changeUserSensitiveContentQueue;

    public override string Id => ProviderId;

    public ChangeUserSensitiveContentMaintenanceProvider(
        IOptions<ChangeUserSensitiveContentMaintenanceOptions> options,
        ISession session,
        ShellSettings shellSettings,
        IChangeUserSensitiveContentQueue changeUserSensitiveContentQueue)
    {
        _options = options;
        _session = session;
        _shellSettings = shellSettings;
        _changeUserSensitiveContentQueue = changeUserSensitiveContentQueue;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.IsEnabled &&
            !context.WasLatestExecutionSuccessful() &&
            _options.Value.TenantNames.Replace(" ", string.Empty).SplitByCommas().Contains(_shellSettings.Name));

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var emailExcludeRegex = new Regex(
            _options.Value.EmailExcludePattern,
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(400));

        var users = await _session.Query<User>().ListAsync();
        var filteredUsers = users.Where(user => !emailExcludeRegex.IsMatch(user.Email.Trim())).ToList();
        _changeUserSensitiveContentQueue.Enqueue(filteredUsers);

        if (filteredUsers.Count > 0)
        {
            context.CurrentExecution.SetWarning(
                $"Added {filteredUsers.Count} users to the queue. If you see this warning, the process is either " +
                "pending or it has failed.");
        }
    }
}
