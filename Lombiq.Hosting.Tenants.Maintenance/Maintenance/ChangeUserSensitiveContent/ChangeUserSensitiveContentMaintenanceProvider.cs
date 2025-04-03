using Lombiq.Hosting.Tenants.Maintenance.Extensions;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using RandomNameGeneratorLibrary;
using System;
using System.Diagnostics;
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
    private readonly ILogger<ChangeUserSensitiveContentMaintenanceProvider> _logger;

    public ChangeUserSensitiveContentMaintenanceProvider(
        IOptions<ChangeUserSensitiveContentMaintenanceOptions> options,
        ISession session,
        IPasswordHasher<IUser> passwordHasher,
        ShellSettings shellSettings,
        ILogger<ChangeUserSensitiveContentMaintenanceProvider> logger)
    {
        _options = options;
        _session = session;
        _passwordHasher = passwordHasher;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public override Task<bool> ShouldExecuteAsync(MaintenanceTaskExecutionContext context) =>
        Task.FromResult(
            _options.Value.IsEnabled &&
            !context.WasLatestExecutionSuccessful() &&
            _options.Value.TenantNames.Replace(" ", string.Empty).SplitByCommas().Contains(_shellSettings.Name));

    public override async Task ExecuteAsync(MaintenanceTaskExecutionContext context)
    {
        _logger.LogError("Starting maintenance ChangeUserSensitiveContent.");
        var randomNameGenerator = new PersonNameGenerator();
        var emailExcludeRegex = new Regex(
            _options.Value.EmailExcludePattern,
            RegexOptions.None,
            TimeSpan.FromMilliseconds(400));

        // To have the best performance, we are processing users in batches and then saving them.
        const int batchSize = 50;
        var skip = 0;

        var users = await _session.Query<User>().ListAsync();
        var filteredUsers = users.Where(user => !emailExcludeRegex.IsMatch(user.Email.Trim())).ToList();

        var passwordHash = _passwordHasher.HashPassword(user: null, GenerateRandomPassword(32));
        var stopwatch = Stopwatch.StartNew();

        stopwatch.Restart();
        _logger.LogError("Entering while loop.");

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

            _logger.LogError("In the while loop, run count: skip count {Skip}.", skip);
        }

        stopwatch.Stop();
        _logger.LogError("SaveChangesAsync user loop completed in {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
    }

    private static string GetFormattedFullName(string firstName, string lastName) => $"{firstName}.{lastName}";

    private static string GetFormattedEmail(string firstName, string lastName) =>
        $"{firstName}.{lastName}@test.com";
}
