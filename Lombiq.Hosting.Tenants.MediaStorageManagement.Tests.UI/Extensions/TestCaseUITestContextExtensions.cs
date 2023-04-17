using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Helpers;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    public static async Task TestMediaStorageManagementBehaviorAsync(this UITestContext context)
    {
        await context.SignInDirectlyAsync();

        await context.GoToAdminRelativeUrlAsync("/Media");

        context.UploadSamplePngByIdOfAnyVisibility("fileupload"); // #spell-check-ignore-line

        // Workaround for pending uploads, until you make an action the page is stuck on "Uploads Pending".
        context.WaitForPageLoad();
        await context.ClickReliablyOnAsync(By.CssSelector("body"));
        await context.ClickReliablyOnAsync(By.CssSelector(".text-danger"));
        context.Get(By.CssSelector(".error-message")).Text.ShouldContain("Error: You may only store");
    }
}
