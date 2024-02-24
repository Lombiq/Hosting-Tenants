using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Management.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    public static async Task TestShellSettingsEditorFeatureAsync(this UITestContext context)
    {
        await context.SignInDirectlyAsync();
        await context.GoToAdminRelativeUrlAsync("/Tenants/Edit/Default");

        // Expected JSON string.
#pragma warning disable JSON002 // Probable JSON string detected
        await context.FillInEditorThenCheckValueAsync(
            "{\"TestKey\":{\"TestSubKey\":{\"TestSubOptions\":{\"FirstTestKey\": \"FirstTestValue\",\"SecondTestKey\": \"SecondTestValue\"}}}}",
            "FirstTestKey",
            "FirstTestValue");

        await context.FillInEditorThenCheckValueAsync(
            "{\"TestKey\":{\"TestSubKey\":{\"TestSubOptions\":{\"NewKey\": \"NewValue\",\"SecondTestKey\": \"SecondTestValue\"}}}}",
            "NewKey",
            "NewValue");

        await context.FillInEditorThenCheckValueAsync(
            "{\"TestKey\":{\"TestSubKey\":{\"TestSubOptions\":{\"SecondTestKey\": \"SecondTestValue\"}}}}",
            "NewKey",
            string.Empty);

        CheckEditorValue(context, "SecondTestKey", "SecondTestValue");

        await context.FillInEditorThenCheckValueAsync(
            "{\"TestKey\":{\"TestSubKey\":{\"TestSubOptions\":{}}}}",
            "SecondTestKey",
            string.Empty);
#pragma warning restore JSON002 // Probable JSON string detected

        await context.FillInEditorThenCheckValueAsync(
            string.Empty,
            "SecondTestKey",
            string.Empty);
    }

    private static async Task FillInEditorThenCheckValueAsync(this UITestContext context, string text, string keyToCheck, string expectedValue)
    {
        context.FillInMonacoEditor("Json_editor", text);
        await context.ClickReliablyOnAsync(By.XPath("//button[contains(.,'Save settings')]"));
        CheckEditorValue(context, keyToCheck, expectedValue);
    }

    private static void CheckEditorValue(this UITestContext context, string keyToCheck, string expectedValue)
    {
        var editorText = context.GetMonacoEditorText("Json_editor");
        var editorJson = string.IsNullOrEmpty(editorText) ? "{}" : editorText;

        var editorValue = JsonNode
            .Parse(editorJson)
            .SelectNode($"TestKey.TestSubKey.TestSubOptions.{keyToCheck}")?
            .ToString();
        editorValue.ShouldNotBeNull();
        editorValue.ShouldBeAsString(expectedValue);
    }
}
