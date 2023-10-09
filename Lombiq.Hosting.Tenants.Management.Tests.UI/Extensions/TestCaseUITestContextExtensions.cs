using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using Shouldly;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Management.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    public static async Task TestShellSettingsEditorFeatureAsync(this UITestContext context)
    {
        await context.SignInDirectlyAsync();
        await context.GoToAdminRelativeUrlAsync("/Tenants/Edit/Default");

        // Expected JSON string
#pragma warning disable JSON002 // Probable JSON string detected
        await context.FillInEditorThenCheckValueAsync(
            "{\"TestKey\":{\"TestSubKey\":{\"TestSubOptions\":{\"Test\": \"TestValue\"}}}}",
            "TestValue");
#pragma warning restore JSON002 // Probable JSON string detected
        await context.FillInEditorThenCheckValueAsync(
            string.Empty,
            expectedValue: null);
    }

    private static async Task FillInEditorThenCheckValueAsync(this UITestContext context, string text, string expectedValue)
    {
        context.FillInMonacoEditor("Json_editor", text);
        await context.ClickReliablyOnAsync(By.XPath("//button[contains(.,'Save settings')]"));
        var editorText = context.GetMonacoEditorText("Json_editor");

        if (string.IsNullOrEmpty(text))
        {
            editorText.ShouldBeAsString(text);
        }
        else
        {
            var editorValue = JObject.Parse(context.GetMonacoEditorText("Json_editor"));
            editorValue.SelectToken("TestKey.TestSubKey.TestSubOptions.Test")?.ToString().ShouldBeAsString(expectedValue);
        }
    }
}
