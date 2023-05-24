using Atata;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.EnvironmentRobots.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    public static async Task TestRobotMetaTagIsMissingAsync(this UITestContext context, bool shouldBeMissing)
    {
        var metaTagXPath = By.XPath($"//meta[@name='robots' and @content='noindex, nofollow']").OfAnyVisibility();

        await context.SignInDirectlyAsync();
        await context.GoToHomePageAsync();

        // The easiest way to check the response header during UI testing is with JavaScript by sending a GET request.
        var isHeaderPresent = context.Driver.ExecuteAsyncScript(@"
            const callback = arguments[arguments.length - 1];
            const xhr = new XMLHttpRequest();
            xhr.open('GET', window.location.href);
            xhr.send();
            xhr.onload = function() {
                const xRobotsTag = xhr.getResponseHeader('X-Robots-Tag') ?? '';
                callback(xRobotsTag.includes('noindex, nofollow'));
            };");

        if (shouldBeMissing)
        {
            context.Missing(metaTagXPath);
            isHeaderPresent.ShouldBe(expected: false);
        }
        else
        {
            context.Exists(metaTagXPath);
            isHeaderPresent.ShouldBe(expected: true);
        }
    }
}
