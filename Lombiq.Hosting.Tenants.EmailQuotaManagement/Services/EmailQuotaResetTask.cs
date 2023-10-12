using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

[BackgroundTask(
    Schedule = "0 0 1 * *",
    Description = "Resets the email quota every new month.")]
public class EmailQuotaResetBackgroundTask : IBackgroundTask
{
    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var emailQuotaService = serviceProvider.GetRequiredService<IEmailQuotaService>();
        var currentQuota = await emailQuotaService.GetOrCreateCurrentQuotaAsync();
        emailQuotaService.ResetQuota(currentQuota);
    }
}
