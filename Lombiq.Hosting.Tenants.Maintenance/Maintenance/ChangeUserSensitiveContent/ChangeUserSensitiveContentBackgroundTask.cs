#nullable enable

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using RandomNameGeneratorLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YesSql;
using static Lombiq.HelpfulLibraries.OrchardCore.Users.PasswordHelper;

namespace Lombiq.Hosting.Tenants.Maintenance.Maintenance.ChangeUserSensitiveContent;

[BackgroundTask(
    Schedule = "* * * * *",
    Description = $"Check on the {nameof(IChangeUserSensitiveContentQueue)} to see if there are users to change.")]
public sealed class ChangeUserSensitiveContentBackgroundTask : IBackgroundTask
{
    private readonly IChangeUserSensitiveContentQueue _queue;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ChangeUserSensitiveContentBackgroundTask(
        IChangeUserSensitiveContentQueue queue,
        IServiceScopeFactory serviceScopeFactory)
    {
        _queue = queue;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        if (await _queue.DequeueAsync() is not { Count: > 0 } firstBatch)
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var provider = scope.ServiceProvider;

        var session = provider.GetRequiredService<ISession>();
        var passwordHasher = provider.GetRequiredService<IPasswordHasher<IUser>>();
        var options = provider.GetRequiredService<IOptions<ChangeUserSensitiveContentMaintenanceOptions>>();

        var randomNameGenerator = new PersonNameGenerator();
        var passwordHash = GeneratePasswordHash(passwordHasher);
        var domainName = options.Value.TargetEmailDomainName;


        await ExecuteAsync(session, firstBatch, randomNameGenerator, passwordHash, domainName, cancellationToken);
        while (await _queue.DequeueAsync() is { Count: > 0 } batch)
        {
            await ExecuteAsync(session, batch, randomNameGenerator, passwordHash, domainName, cancellationToken);
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
