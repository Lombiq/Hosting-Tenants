using Lombiq.Hosting.Tenants.Maintenance.Maintenance.PurgeMediaCache;
using Lombiq.Hosting.Tenants.Maintenance.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Media;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Lombiq.Hosting.Tenants.Maintenance.Tests.Maintenance;

public sealed class PurgeMediaCacheMaintenanceProviderTests : IDisposable
{
    private ServiceProvider _serviceProvider;

    [Theory]
    [InlineData(false, "1.0.0", "2.0.0", false)]
    [InlineData(true, "1.0.0", "1.0.0", false)]
    [InlineData(true, "1.0.0", "2.0.0", true)]
    [InlineData(true, null, "2.0.0", true)]
    public async Task ShouldExecuteShouldConsiderOptionsAndBuildVersion(
        bool isEnabled,
        string latestBuildVersion,
        string currentBuildVersion,
        bool expected)
    {
        var provider = new PurgeMediaCacheMaintenanceProvider(
            Options.Create(new PurgeMediaCacheMaintenanceOptions { IsEnabled = isEnabled }),
            CreateServiceProvider());

        var context = new MaintenanceTaskExecutionContext
        {
            LatestExecution = latestBuildVersion == null
                ? null
                : new MaintenanceTaskExecutionData { BuildVersion = latestBuildVersion },
            CurrentExecution = new MaintenanceTaskExecutionData { BuildVersion = currentBuildVersion },
        };

        var shouldExecute = await provider.ShouldExecuteAsync(context);

        shouldExecute.ShouldBe(expected);
    }

    [Fact]
    public async Task ExecuteShouldPurgeCacheWhenProviderRegistered()
    {
        var cacheProviderMock = new Mock<IMediaFileStoreCacheFileProvider>(MockBehavior.Strict);
        cacheProviderMock.Setup(provider => provider.PurgeAsync()).Returns(Task.FromResult(true)).Verifiable();

        var provider = new PurgeMediaCacheMaintenanceProvider(
            Options.Create(new PurgeMediaCacheMaintenanceOptions { IsEnabled = true }),
            CreateServiceProvider(cacheProviderMock.Object));

        await provider.ExecuteAsync(new MaintenanceTaskExecutionContext());

        cacheProviderMock.Verify(provider => provider.PurgeAsync(), Times.Once);
    }

    [Fact]
    public Task ExecuteShouldCompleteWhenCacheProviderMissing()
    {
        var provider = new PurgeMediaCacheMaintenanceProvider(
            Options.Create(new PurgeMediaCacheMaintenanceOptions { IsEnabled = true }),
            CreateServiceProvider());

        return provider.ExecuteAsync(new MaintenanceTaskExecutionContext());
    }

    public void Dispose() => _serviceProvider?.Dispose();

    private ServiceProvider CreateServiceProvider(IMediaFileStoreCacheFileProvider cacheProvider = null)
    {
        var services = new ServiceCollection();

        if (cacheProvider != null) services.AddSingleton(cacheProvider);

        return _serviceProvider = services.BuildServiceProvider();
    }
}
