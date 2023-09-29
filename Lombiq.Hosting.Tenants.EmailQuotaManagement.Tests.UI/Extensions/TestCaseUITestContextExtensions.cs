using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Helpers;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    private const string SuccessfulSubject = "Successful test message";
    private const string UnSuccessfulSubject = "Unsuccessful test message";
    private const string DashboardWarning =
        "//p[contains(@class,'alert-danger')][contains(.,'It seems that your site sent out more e-mails')]";

    public static async Task TestEmailQuotaManagementBehaviorAsync(
        this UITestContext context,
        int maximumEmailQuota,
        bool moduleShouldInterfere = true)
    {
        await context.SignInDirectlyAndGoToDashboardAsync();

        context.Missing(By.XPath(DashboardWarning));

        await context.GoToAdminRelativeUrlAsync("/Settings/email");

        CheckEmailsSentWarningMessage(context, exists: moduleShouldInterfere, maximumEmailQuota, 0);

        await SendTestEmailAsync(context, SuccessfulSubject);
        context.SuccessMessageExists();

        CheckEmailsSentWarningMessage(context, exists: moduleShouldInterfere, maximumEmailQuota, 1);

        await context.GoToDashboardAsync();
        context.CheckExistence(By.XPath(DashboardWarning), exists: moduleShouldInterfere);

        await SendTestEmailAsync(context, UnSuccessfulSubject);
        await context.GoToSmtpWebUIAsync();
        context.CheckExistence(ByHelper.SmtpInboxRow(SuccessfulSubject), exists: true);
        context.CheckExistence(
            ByHelper.SmtpInboxRow("[Action Required] Your DotNest site has run over its e-mail quota"),
            exists: moduleShouldInterfere);
        context.CheckExistence(ByHelper.SmtpInboxRow(UnSuccessfulSubject), exists: !moduleShouldInterfere);
    }

    private static void CheckEmailsSentWarningMessage(UITestContext context, bool exists, int maximumEmailQuota, int currentEmailCount) =>
        context.CheckExistence(
            By.XPath($"//p[contains(@class,'alert-warning')][contains(.,'{currentEmailCount.ToTechnicalString()} emails" +
                $" from the total of {maximumEmailQuota.ToTechnicalString()}.')]"),
            exists);

    private static async Task SendTestEmailAsync(UITestContext context, string subject)
    {
        await context.GoToAdminRelativeUrlAsync("/Email/Index");
        await context.FillInWithRetriesAsync(By.Id("To"), "recipient@example.com");
        await context.FillInWithRetriesAsync(By.Id("Subject"), subject);
        await context.FillInWithRetriesAsync(By.Id("Body"), "Hi, this is a test.");

        await ReliabilityHelper.DoWithRetriesOrFailAsync(
            async () =>
            {
                try
                {
                    await context.ClickReliablyOnAsync(By.Id("emailtestsend")); // #spell-check-ignore-line
                    return true;
                }
                catch (WebDriverException ex) when (ex.Message.Contains("move target out of bounds"))
                {
                    return false;
                }
            });
    }
}
