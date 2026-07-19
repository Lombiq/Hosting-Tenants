using Atata;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    public static async Task TestMediaStorageManagementBehaviorAsync(this UITestContext context, int uploadSizeMegabytes)
    {
        await context.SignInDirectlyAsync();

        await context.GoToAdminRelativeUrlAsync("/Media");

        var uploadFilePath = context.GetTempSubDirectoryPath("big-file.zip");

        try
        {
            // Generate a temporary large zip file.
            await using (var file = File.OpenWrite(uploadFilePath))
            await using (var zip = new ZipArchive(file, ZipArchiveMode.Create))
            {
                for (var i = 0; i < uploadSizeMegabytes; i++)
                {
                    // Generate 1 MB entries with no compression.
                    var entry = zip.CreateEntry(i.ToTechnicalString(), CompressionLevel.NoCompression);
                    await using var writer = new StreamWriter(await entry.OpenAsync(context.Configuration.TestCancellationToken));
                    await writer.WriteAsync(new char[1024 * 1024]);
                }
            }

            context.UploadFileByIdOfAnyVisibility("fileupload", uploadFilePath);

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
            context
                .Get(By.CssSelector(".error-message"))
                .Text
                .ShouldContain($"Error: You may only store {uploadSizeMegabytes.ToTechnicalString()} MB");
        }
        finally
        {
            // Always clean this file up, because it's a waste of space and wouldn't help with debugging anyway.
            if (File.Exists(uploadFilePath))
            {
                File.Delete(uploadFilePath);
            }
        }
    }
}
