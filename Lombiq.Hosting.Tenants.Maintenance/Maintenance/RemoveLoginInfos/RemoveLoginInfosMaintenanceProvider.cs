using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.Extensions.Options;
using OrchardCore.Users.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.RemoveLoginInfos;

public class RemoveLoginInfosMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<RemoveLoginInfosMaintenanceOptions> _options;
    private readonly ISession _session;

    public RemoveLoginInfosMaintenanceProvider(
        IOptions<RemoveLoginInfosMaintenanceOptions> options,
        ISession session)
    {
        _options = options;
        _session = session;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.IsEnabled &&
            !context.WasLatestExecutionSuccessful());

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var users = await _session.Query<User>().ListAsync();
        foreach (var user in users)
        {
            user.LoginInfos.RemoveAll();
        }
    }
}
