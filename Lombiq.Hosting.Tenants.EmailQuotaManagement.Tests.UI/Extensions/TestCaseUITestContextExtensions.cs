using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Helpers;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    private const string SuccessfulSubject = "Successful test message";
    private const string UnSuccessfulSubject = "Unsuccessful test message";
    private const string WarningSubject = "[Warning] Your site has used";
    private const string DashboardExceededMessage =
        "//p[contains(@class,'alert-danger')][contains(.,'It seems that your site sent out more e-mails')]";

    public static async Task TestEmailQuotaManagementBehaviorAsync(
        this UITestContext context,
        int maximumEmailQuota,
        bool moduleShouldInterfere = true)
    {
        await context.SignInDirectlyAndGoToDashboardAsync();

        context.Missing(By.XPath(DashboardExceededMessage));

        await context.GoToAdminRelativeUrlAsync("/Settings/email");

        CheckEmailsSentWarningMessage(context, exists: moduleShouldInterfere, maximumEmailQuota, 0);

        var warningEmails = new List<int>();
        for (int i = 0; i < maximumEmailQuota; i++)
        {
            await SendTestEmailAsync(context, SuccessfulSubject);
            context.SuccessMessageExists();
            CheckEmailsSentWarningMessage(context, exists: moduleShouldInterfere, maximumEmailQuota, i + 1);
            var warningLevel = Convert.ToInt32(Math.Round((double)(i + 1) / maximumEmailQuota * 100, 0));

            if (!moduleShouldInterfere) continue;

            if (warningLevel >= 100)
            {
                await context.GoToDashboardAsync();
                context.CheckExistence(By.XPath(DashboardExceededMessage), exists: true);
            }
            else if (warningLevel >= 80)
            {
                await context.GoToDashboardAsync();
                CheckMessageExistence(context, warningLevel.ToTechnicalString());

                await context.GoToContentItemsPageAsync();
                CheckMessageExistence(context, warningLevel.ToTechnicalString());

                await context.GoToFeaturesPageAsync();
                CheckMessageExistence(context, warningLevel.ToTechnicalString());

                if (!warningEmails.Contains(warningLevel))
                {
                    warningEmails.Add(warningLevel);
                }
            }
        }

        await SendTestEmailAsync(context, UnSuccessfulSubject);
        await context.GoToSmtpWebUIAsync();
        context.CheckExistence(ByHelper.SmtpInboxRow(SuccessfulSubject), exists: true);
        context.CheckExistence(
            ByHelper.SmtpInboxRow("[Action Required] Your site has run over its e-mail quota"),
            exists: moduleShouldInterfere);
        var warningMessageExists = context.CheckExistence(
            ByHelper.SmtpInboxRow(WarningSubject),
            exists: moduleShouldInterfere);
        if (moduleShouldInterfere && warningMessageExists)
        {
            (context.GetAll(
                ByHelper.SmtpInboxRow(WarningSubject)).Count == warningEmails.Count)
                .ShouldBeTrue();
        }

        context.CheckExistence(ByHelper.SmtpInboxRow(UnSuccessfulSubject), exists: !moduleShouldInterfere);
    }

    private static void CheckMessageExistence(UITestContext context, string warningLevel) =>
        context.CheckExistence(
            By.XPath($"//p[contains(@class,'alert-warning')]" +
                $"[contains(.,'It seems that your site sent out {warningLevel}% of e-mail')]"),
            exists: true);

    private static void CheckEmailsSentWarningMessage(UITestContext context, bool exists, int maximumEmailQuota, int currentEmailCount) =>
        context.CheckExistence(
            By.XPath($"//p[contains(@class,'alert-warning')][contains(.,'{currentEmailCount.ToTechnicalString()} emails" +
                $" from the total of {maximumEmailQuota.ToTechnicalString()} this month.')]"),
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
