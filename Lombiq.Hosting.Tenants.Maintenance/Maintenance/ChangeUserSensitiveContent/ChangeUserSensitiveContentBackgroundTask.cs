#nullable enable

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    Schedule = "*/10 * * * *",
    Description = "Check on the .")]
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

        var session = scope.ServiceProvider.GetRequiredService<ISession>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<IUser>>();

        var randomNameGenerator = new PersonNameGenerator();

        // We don't want to login with these accounts, so we are generating the same password hash for each user, to
        // make the process faster.
        var passwordHash = passwordHasher.HashPassword(new User(), GenerateRandomPassword(32));

        await ExecuteAsync(session, firstBatch, randomNameGenerator, passwordHash, cancellationToken);
        while (await _queue.DequeueAsync() is { Count: > 0 } batch)
        {
            await ExecuteAsync(session, batch, randomNameGenerator, passwordHash, cancellationToken);
        }
    }

    private static async Task ExecuteAsync(
        ISession session,
        ICollection<User> batch,
        PersonNameGenerator randomNameGenerator,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        foreach (var user in batch)
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

            await session.SaveAsync(user, cancellationToken: cancellationToken);
        }

        await session.SaveChangesAsync(cancellationToken);
    }

    private static string GetFormattedFullName(string firstName, string lastName) =>
        $"{firstName} {lastName}";

    private static string GetFormattedEmail(string firstName, string lastName) =>
        $"{firstName}{lastName}@test.com";
}
