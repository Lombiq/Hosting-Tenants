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

        // Checking the response header with JavaScript.
        var isHeaderPresent = context.Driver.ExecuteAsyncScript(@"
            var callback = arguments[arguments.length - 1];
            var xhr = new XMLHttpRequest();
            xhr.open('GET', window.location.href);
            xhr.send();
            xhr.onload = function() {
                var xRobotsTag = xhr.getResponseHeader('X-Robots-Tag') ?? '';
                callback(xRobotsTag.includes ('noindex, nofollow'));
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
