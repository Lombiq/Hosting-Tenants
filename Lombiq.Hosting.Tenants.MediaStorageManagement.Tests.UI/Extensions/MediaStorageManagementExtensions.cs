using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
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
                        "OrchardCore:Lombiq_Hosting_Tenants_MediaStorageManagement:MaximumStorageQuotaBytes",
                        maximumStorageQuotaBytes);

                return Task.CompletedTask;
            };

        // By default, apart from some commonly known false positives, the response log should be empty. However, Media
        // Storage Quota feature causes a 400 on upload if the file is over the limit, so we need to make sure not to
        // fail on that.
        configuration.ResponseLogFilter = e =>
            e.IsNonSuccessResponse() &&
            e.IsNonSuccessResponseAndNotExpectedStatusResponse("/Admin/Media/Upload", 400);
    }
}
