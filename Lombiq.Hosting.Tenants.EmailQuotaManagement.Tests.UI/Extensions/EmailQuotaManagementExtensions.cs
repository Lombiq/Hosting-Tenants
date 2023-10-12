using Lombiq.Tests.UI.Services;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Tests.UI.Extensions;

public static class EmailQuotaManagementExtensions
{
    public static void SetEmailQuotaManagementOptionsForUITest(
        this OrchardCoreUITestExecutorConfiguration configuration,
        long maximumEmails,
        string emailHost = "localhost")
    {
        configuration.OrchardCoreConfiguration.BeforeAppStart +=
            (_, argumentsBuilder) =>
            {
                argumentsBuilder
                    .AddWithValue(
                        "OrchardCore:Lombiq_Hosting_Tenants_EmailQuotaManagement:EmailQuotaPerMonth",
                        maximumEmails);

                argumentsBuilder
                    .AddWithValue(
                        "OrchardCore:SmtpSettings:Host",
                        emailHost);

                return Task.CompletedTask;
            };

        configuration.UseSmtpService = true;
    }
}
