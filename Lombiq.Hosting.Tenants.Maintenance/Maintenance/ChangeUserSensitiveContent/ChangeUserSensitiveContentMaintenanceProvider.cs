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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YesSql;
using static Lombiq.HelpfulLibraries.OrchardCore.Users.PasswordHelper;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ChangeUserSensitiveContent;

public class ChangeUserSensitiveContentMaintenanceProvider : MaintenanceProviderBase
{
    private readonly IOptions<ChangeUserSensitiveContentMaintenanceOptions> _options;
    private readonly ISession _session;
    private readonly IPasswordHasher<IUser> _passwordHasher;
    private readonly ShellSettings _shellSettings;

    public ChangeUserSensitiveContentMaintenanceProvider(
        IOptions<ChangeUserSensitiveContentMaintenanceOptions> options,
        ISession session,
        IPasswordHasher<IUser> passwordHasher,
        ShellSettings shellSettings)
    {
        _options = options;
        _session = session;
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
        var emailExcludeRegex = new Regex(
            _options.Value.EmailExcludePattern,
            RegexOptions.None,
            TimeSpan.FromMilliseconds(400));

        // To have the best performance, we are processing users in batches of 15 and then saving them. Multiple batch
        // sizes were tried but 15 seems to grant the best performance.
        const int batchSize = 15;

        var users = await _session.Query<User>().ListAsync();
        var filteredUsers = users.Where(user => !emailExcludeRegex.IsMatch(user.Email.Trim())).ToList();

        // We don't want to login with these accounts, so we are generating the same password hash for each user, to
        // make the process faster.
        var passwordHash = _passwordHasher.HashPassword(user: null, GenerateRandomPassword(32));

        var skip = 0;

        while (skip < filteredUsers.Count)
        {
            var filteredUsersBatch = filteredUsers
                .Skip(skip)
                .Take(batchSize);

            foreach (var user in filteredUsersBatch)
            {
                var firstName = randomNameGenerator.GenerateRandomFirstName();
                var lastName = randomNameGenerator.GenerateRandomLastName();

                var formattedFullName = GetFormattedFullName(firstName, lastName);
                var formattedEmail = GetFormattedEmail(firstName, lastName);

                user.UserName = formattedFullName;
                user.NormalizedUserName = formattedFullName.ToUpperInvariant();
                user.Email = formattedEmail;
                user.NormalizedEmail = formattedEmail.ToUpperInvariant();

                user.PasswordHash = passwordHash;

                await _session.SaveAsync(user);
            }

            await _session.SaveChangesAsync();

            skip += batchSize;
        }
    }

    private static string GetFormattedFullName(string firstName, string lastName) => $"{firstName}.{lastName}";

    private static string GetFormattedEmail(string firstName, string lastName) =>
        $"{firstName}.{lastName}@test.com";
}
