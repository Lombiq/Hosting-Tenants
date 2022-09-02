using Lombiq.Hosting.MultiTenancy.Tenants.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using System.Threading.Tasks;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Services;

public class FeaturesGuardService : IFeaturesGuardService
{
    private readonly RequestDelegate _next;
    private readonly OrchardCoreBuilder _orchardCoreBuilder;

    public FeaturesGuardService(
        RequestDelegate next,
        IExtensionManager extensionManager,
        OrchardCoreBuilder orchardCoreBuilder)
    {
        _next = next;
        _orchardCoreBuilder = orchardCoreBuilder;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IOptions<ForbiddenFeaturesOptions> forbiddenOptions,
        IOptions<AlwaysOnFeaturesOptions> alwaysOnOptions,
        IShellFeaturesManager shellFeaturesManager)
    {
        // this should only run on user tenants -- and likely only during setup?
        await EnableFeatures(context, alwaysOnOptions);

        await _next.Invoke(context);
    }

    public Task EnableFeatures(HttpContext context, IOptions<AlwaysOnFeaturesOptions> options)
    {
        var alwaysOnFeatures = options.Value.AlwaysOnFeatures;
        foreach (var feature in alwaysOnFeatures)
        {
            _orchardCoreBuilder.EnableFeature(feature);
        }

        return Task.CompletedTask;
    }
}
