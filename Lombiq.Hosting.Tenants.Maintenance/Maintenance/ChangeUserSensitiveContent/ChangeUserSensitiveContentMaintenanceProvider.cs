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
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using YesSql;

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

            user.UserName = GetFormattedFullName(firstName, lastName);
            user.NormalizedUserName = GetFormattedFullName(firstName, lastName).ToUpperInvariant();
            user.Email = GetFormattedEmail(firstName, lastName);
            user.NormalizedEmail = GetFormattedEmail(firstName, lastName).ToUpperInvariant();
            user.PasswordHash = _passwordHasher.HashPassword(user, GenerateRandomPassword(32));

            await _userManager.UpdateAsync(user);
        }
    }

    private static string GetFormattedFullName(string firstName, string lastName) => $"{firstName}.{lastName}";

    private static string GetFormattedEmail(string firstName, string lastName) =>
        $"{firstName}.{lastName}@test.com";

    private static string GenerateRandomPassword(int minLength)
    {
        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-="; // #spell-check-ignore-line

        using var rng = RandomNumberGenerator.Create();
        const string digits = "0123456789";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz"; // #spell-check-ignore-line
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // #spell-check-ignore-line
        const string specialChars = "!@#$%^&*()_+-=";

        var passwordChars = new List<char>
            {
                digits[rng.Next(0, digits.Length)],
                lowerChars[rng.Next(0, lowerChars.Length)],
                upperChars[rng.Next(0, upperChars.Length)],
                specialChars[rng.Next(0, specialChars.Length)],
            };

        while (passwordChars.Count < minLength)
        {
            passwordChars.Add(validChars[rng.Next(0, validChars.Length)]);
        }

        passwordChars = passwordChars.OrderBy(c => rng.Next(0, int.MaxValue)).ToList();
        string password = new(passwordChars.ToArray());

        return password;
    }
}
