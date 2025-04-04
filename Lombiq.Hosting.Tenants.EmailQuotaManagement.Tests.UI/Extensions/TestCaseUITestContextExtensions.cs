using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Helpers;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        bool quotaShouldBeEnforced = true)
    {
        await context.SignInDirectlyAndGoToDashboardAsync();
        context.Missing(By.XPath(DashboardExceededMessage));

        await context.GoToEmailSettingsAsync();
        await context.ClickReliablyOnAsync(By.ClassName("save"));

        var warningEmails = new List<int>();
        var quotaAwareEmailCount = 0;
        for (int i = 0; i < maximumEmailQuota; i++)
        {

            await context.GoToEmailTestAsync();
            await context.FillEmailTestFormAsync(SuccessfulSubject);
            context.SuccessMessageExists();

            if (quotaShouldBeEnforced)
            {
                quotaAwareEmailCount = i + 1;
            }

            CheckEmailsSentWarningMessage(context, maximumEmailQuota, quotaAwareEmailCount);

            if (!quotaShouldBeEnforced) continue;

            var warningLevel = Convert.ToInt32(Math.Round((double)quotaAwareEmailCount / maximumEmailQuota * 100, 0));
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

        await context.GoToEmailTestAsync();
        await context.FillEmailTestFormAsync(UnSuccessfulSubject);
        await context.GoToSmtpWebUIAsync();
        context.CheckExistence(ByHelper.SmtpInboxRow(SuccessfulSubject), exists: true);
        context.CheckExistence(
            ByHelper.SmtpInboxRow("[Action Required] Your site has run over its e-mail quota"),
            exists: quotaShouldBeEnforced);
        var warningMessageExists = context.CheckExistence(
            ByHelper.SmtpInboxRow(WarningSubject),
            exists: quotaShouldBeEnforced);
        if (quotaShouldBeEnforced && warningMessageExists)
        {
            (context.GetAll(
                ByHelper.SmtpInboxRow(WarningSubject)).Count == warningEmails.Count)
                .ShouldBeTrue();
        }

        context.CheckExistence(ByHelper.SmtpInboxRow(UnSuccessfulSubject), exists: !quotaShouldBeEnforced);
    }

    private static void CheckMessageExistence(UITestContext context, string warningLevel) =>
        context.CheckExistence(
            By.XPath($"//p[contains(@class,'alert-warning')]" +
                $"[contains(.,'It seems that your site sent out {warningLevel}% of e-mail')]"),
            exists: true);

    private static void CheckEmailsSentWarningMessage(UITestContext context, int maximumEmailQuota, int currentEmailCount)
    {
        var by = By.CssSelector(".alert-warning[data-email-quota-max][data-email-quota-used]");

        var element = context.Get(by);
        var max = int.Parse(element.GetAttribute("data-email-quota-max"), CultureInfo.InvariantCulture);
        var used = int.Parse(element.GetAttribute("data-email-quota-used"), CultureInfo.InvariantCulture);

        max.ShouldBe(maximumEmailQuota);
        used.ShouldBe(currentEmailCount);
    }
}
