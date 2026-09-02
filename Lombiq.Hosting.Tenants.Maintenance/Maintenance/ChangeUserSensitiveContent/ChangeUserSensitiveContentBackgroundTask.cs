#nullable enable

using Lombiq.Hosting.Tenants.Maintenance.Constants;
using Lombiq.Hosting.Tenants.Maintenance.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using RandomNameGeneratorLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YesSql;
using static Lombiq.HelpfulLibraries.OrchardCore.Users.PasswordHelper;
using static Lombiq.Hosting.Tenants.Maintenance.Maintenance.ChangeUserSensitiveContent.ChangeUserSensitiveContentMaintenanceProvider;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ChangeUserSensitiveContent;

[BackgroundTask(
    Schedule = "* * * * *",
    Description = $"Check on the {nameof(IChangeUserSensitiveContentQueue)} to see if there are users to change.")]
public sealed class ChangeUserSensitiveContentBackgroundTask : IBackgroundTask
{
    private readonly IChangeUserSensitiveContentQueue _queue;

    public ChangeUserSensitiveContentBackgroundTask(IChangeUserSensitiveContentQueue queue) => _queue = queue;

    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        _queue.Count == 0 ? Task.CompletedTask : DoWorkInnerAsync(serviceProvider, cancellationToken);

    public async Task DoWorkInnerAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // Acquire services.
        var clock = serviceProvider.GetRequiredService<IClock>();
        var maintenanceManager = serviceProvider.GetRequiredService<IMaintenanceManager>();
        var options = serviceProvider.GetRequiredService<IOptions<ChangeUserSensitiveContentMaintenanceOptions>>();
        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher<IUser>>();
        var session = serviceProvider.GetRequiredService<ISession>();

        // Process users in batches.
        var randomNameGenerator = new PersonNameGenerator();
        var passwordHash = GeneratePasswordHash(passwordHasher);
        var domainName = options.Value.TargetEmailDomainName;
        while (await _queue.DequeueAsync() is { Count: > 0 } batch)
        {
            await ExecuteAsync(session, batch, randomNameGenerator, passwordHash, domainName, cancellationToken);
        }

        // If we reached this point, we can remove the warning from the latest execution.
        if (await maintenanceManager.GetLatestExecutionByMaintenanceIdAsync(ProviderId) is { } execution)
        {
            execution.Error = null;
            execution.ExecutionEndUtc = clock.UtcNow;
            await session.SaveAsync(
                execution,
                collection: DocumentCollections.Maintenance,
                cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Generates a useless but valid password hash. We don't want to log in with these accounts, so we can generate the
    /// same password hash for each user to make the process faster.
    /// </summary>
    private static string GeneratePasswordHash(IPasswordHasher<IUser> passwordHasher) =>
        passwordHasher.HashPassword(new User(), GenerateRandomPassword(32));

    private static async Task ExecuteAsync(
        ISession session,
        ICollection<User> batch,
        PersonNameGenerator randomNameGenerator,
        string passwordHash,
        string domainName,
        CancellationToken cancellationToken)
    {
        foreach (var user in batch)
        {
            var firstName = randomNameGenerator.GenerateRandomFirstName();
            var lastName = randomNameGenerator.GenerateRandomLastName();

            var formattedFullName = $"{firstName} {lastName}";
            var formattedEmail = $"{firstName}{lastName}@{domainName}";

            user.UserName = formattedFullName;
            user.NormalizedUserName = formattedFullName.ToUpperInvariant();
            user.Email = formattedEmail;
            user.NormalizedEmail = formattedEmail.ToUpperInvariant();

            user.PasswordHash = passwordHash;

            await session.SaveAsync(user, cancellationToken: cancellationToken);
        }

        await session.SaveChangesAsync(cancellationToken);
    }
}
