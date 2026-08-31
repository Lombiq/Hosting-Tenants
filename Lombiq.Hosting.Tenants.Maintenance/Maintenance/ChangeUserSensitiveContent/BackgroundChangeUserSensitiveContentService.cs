#nullable enable

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

public sealed class BackgroundChangeUserSensitiveContentService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BackgroundChangeUserSensitiveContentService(IServiceScopeFactory serviceScopeFactory) =>
        _serviceScopeFactory = serviceScopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var queue = scope.ServiceProvider.GetRequiredService<IChangeUserSensitiveContentQueue>();
            var session = scope.ServiceProvider.GetRequiredService<ISession>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<IUser>>();

            var randomNameGenerator = new PersonNameGenerator();

            // We don't want to login with these accounts, so we are generating the same password hash for each user, to
            // make the process faster.
            var passwordHash = passwordHasher.HashPassword(new User(), GenerateRandomPassword(32));

            while (await queue.DequeueAsync() is { Count: > 0 } batch)
            {
                await ExecuteAsync(session, batch, randomNameGenerator, passwordHash, stoppingToken);
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private static async Task ExecuteAsync(
        ISession session,
        ICollection<User> batch,
        PersonNameGenerator randomNameGenerator,
        string passwordHash,
        CancellationToken stoppingToken)
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

            await session.SaveAsync(user, cancellationToken: stoppingToken);
        }

        await session.SaveChangesAsync(stoppingToken);
    }

    private static string GetFormattedFullName(string firstName, string lastName) =>
        $"{firstName} {lastName}";

    private static string GetFormattedEmail(string firstName, string lastName) =>
        $"{firstName}{lastName}@test.com";
}
