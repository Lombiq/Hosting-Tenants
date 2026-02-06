using Atata;
using Lombiq.Tests.UI.Extensions;
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

        context.UploadSamplePngByIdOfAnyVisibility("fileupload");

        // Wait for upload without blocking, until you make an action the page is stuck on "Uploads Pending".
        await context.DoWithRetriesOrFailAsync(async () =>
        {
            await context.ClickReliablyOnAsync(By.CssSelector("body"));
            var isPending =
                context.Get(By.ClassName("upload-list").Safely()) is not { } uploadList ||
                !uploadList.Text.Contains("(Pending: 1)");
            return isPending || context.Exists(By.CssSelector(".text-danger").Safely());
        });

        await context.ClickReliablyOnAsync(By.CssSelector(".text-danger"));
        context.Get(By.CssSelector(".error-message")).Text.ShouldContain("Error: You may only store");
    }
}
