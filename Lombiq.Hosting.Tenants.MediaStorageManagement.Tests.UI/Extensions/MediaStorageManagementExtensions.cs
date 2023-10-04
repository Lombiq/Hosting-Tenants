using Lombiq.Tests.UI.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Tests.UI.Extensions;

public static class MediaStorageManagementExtensions
{
    public static void SetMediaStorageManagementOptionsForUITest(
        this OrchardCoreUITestExecutorConfiguration configuration,
        long maximumStorageQuotaBytes)
    {
        configuration.OrchardCoreConfiguration.BeforeAppStart +=
            (_, argumentsBuilder) =>
            {
                argumentsBuilder
                    .AddWithValue(
                        "OrchardCore:Default:Lombiq_Hosting_Tenants_MediaStorageManagement:MaximumStorageQuotaBytes",
                        maximumStorageQuotaBytes);

                return Task.CompletedTask;
            };

        configuration.AssertBrowserLog =
            logEntries =>
            {
                // By default, apart from some commonly known exceptions, the browser log should be empty. However,
                // Media Storage Quota feature causes a 400 on upload if the file is over the limit, so we need to make
                // sure not to fail on that.
                var messagesWithoutToggle = logEntries.Where(logEntry =>
                    !logEntry.Message.ContainsOrdinalIgnoreCase(
                        @"/Admin/Media/Upload - Failed to load resource: the server responded with a status of 400"));
                OrchardCoreUITestExecutorConfiguration.AssertBrowserLogIsEmpty(messagesWithoutToggle);
            };
    }
}
