using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using Microsoft.Extensions.Logging;
using OrchardCore.FileStorage;
using OrchardCore.Media.Core.Helpers;
using System.Threading.Tasks;
using Xunit;

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
                        "OrchardCore:Lombiq_Hosting_Tenants_MediaStorageManagement:MaximumStorageQuotaBytes",
                        maximumStorageQuotaBytes);

                return Task.CompletedTask;
            };

        // Exceeding the upload file upload limit causes an error log. This is expected.
        var permittedErrorMessage =
            $"You tried to upload a file that requires {FileSizeHelpers.FormatAsBytes(maximumStorageQuotaBytes)}";
        configuration.AssertAppLogsAsync = app => app.LogsShouldNotContainAsync(
            logEntry =>
                logEntry.Level >= LogLevel.Error &&
                !(logEntry.Exception is FileStoreException && logEntry.Exception.Message.Contains(permittedErrorMessage)),
            TestContext.Current.CancellationToken);
    }
}
