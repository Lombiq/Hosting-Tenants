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

public class ChangeUserSensitiveContentMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<ChangeUserSensitiveContentMaintenanceOptions> _options;
    private readonly ISession _session;
    private readonly UserManager<IUser> _userManager;
    private readonly IPasswordHasher<IUser> _passwordHasher;
    private readonly ShellSettings _shellSettings;

    public ChangeUserSensitiveContentMaintenanceProvider(
        IOptions<ChangeUserSensitiveContentMaintenanceOptions> options,
        ISession session,
        UserManager<IUser> userManager,
        IPasswordHasher<IUser> passwordHasher,
        ShellSettings shellSettings)
    {
        _options = options;
        _session = session;
        _userManager = userManager;
        _passwordHasher = passwordHasher;
        _shellSettings = shellSettings;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.IsEnabled &&
            !context.WasLatestExecutionSuccessful() &&
            _options.Value.TenantNames.Replace(" ", string.Empty).SplitByCommas().Contains(_shellSettings.Name));

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        var randomNameGenerator = new PersonNameGenerator();

        var users = await _session.Query<User>().ListAsync();
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
            user.PasswordHash = _passwordHasher.HashPassword(user, GenerateRandomPassword(32));

            await _userManager.UpdateAsync(user);
        }
    }

    private static string GetFormattedFullName(string firstName, string lastName) => $"{firstName}.{lastName}";

    private static string GetFormattedEmail(string firstName, string lastName) =>
        $"{firstName}.{lastName}@test.com";
}
