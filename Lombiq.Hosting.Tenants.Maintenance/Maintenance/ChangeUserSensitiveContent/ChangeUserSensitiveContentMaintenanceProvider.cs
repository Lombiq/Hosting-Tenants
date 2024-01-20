using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using RandomNameGeneratorLibrary;
using System;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using static Lombiq.HelpfulLibraries.OrchardCore.Users.PasswordHelper;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ChangeUserSensitiveContent;

public class ChangeUserSensitiveContentMaintenanceProvider(
    IOptions<ChangeUserSensitiveContentMaintenanceOptions> options,
    ISession session,
    UserManager<IUser> userManager,
    IPasswordHasher<IUser> passwordHasher,
    ShellSettings shellSettings) : MaintenanceProviderBase
{
    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            options.Value.IsEnabled &&
            !context.WasLatestExecutionSuccessful() &&
            options.Value.TenantNames.Replace(" ", string.Empty).SplitByCommas().Contains(shellSettings.Name));

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var randomNameGenerator = new PersonNameGenerator();

        var users = await session.Query<User>().ListAsync();
        foreach (var user in users.Where(user =>
            !user.Email.Trim().EndsWith($"@lombiq.com", StringComparison.InvariantCulture)))
        {
            var firstName = randomNameGenerator.GenerateRandomFirstName();
            var lastName = randomNameGenerator.GenerateRandomLastName();
            var formattedFullName = GetFormattedFullName(firstName, lastName);
            var formattedEmail = GetFormattedEmail(firstName, lastName);

            user.UserName = formattedFullName;
            user.NormalizedUserName = formattedFullName.ToUpperInvariant();
            user.Email = formattedEmail;
            user.NormalizedEmail = formattedEmail.ToUpperInvariant();
            user.PasswordHash = passwordHasher.HashPassword(user, GenerateRandomPassword(32));

            await userManager.UpdateAsync(user);
        }
    }

    private static string GetFormattedFullName(string firstName, string lastName) => $"{firstName}.{lastName}";

    private static string GetFormattedEmail(string firstName, string lastName) =>
        $"{firstName}.{lastName}@test.com";
}
