using Lombiq.Hosting.MultiTenancy.Tenants.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Services;

public class FeaturesGuardService : IFeaturesGuardService
{
    private readonly RequestDelegate _next;
    private readonly IExtensionManager _extensionManager;
    private readonly OrchardCoreBuilder _orchardCoreBuilder;

    public FeaturesGuardService(
        RequestDelegate next,
        IExtensionManager extensionManager,
        OrchardCoreBuilder orchardCoreBuilder)
    {
        _next = next;
        _extensionManager = extensionManager;
        _orchardCoreBuilder = orchardCoreBuilder;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IOptions<ForbiddenFeaturesOptions> forbiddenOptions,
        IOptions<AlwaysOnFeaturesOptions> alwaysOnOptions,
        IShellFeaturesManager shellFeaturesManager)
    {
        await DisableFeatures(context, forbiddenOptions, shellFeaturesManager);
        await EnableFeatures(context, alwaysOnOptions);

        await _next.Invoke(context);
    }

    public async Task DisableFeatures(
        HttpContext context,
        IOptions<ForbiddenFeaturesOptions> options,
        IShellFeaturesManager shellFeaturesManager)
    {
        // this currently runs not only on all requests, but also on all tenants. Should only be run on user tenants

        var forbiddenFeatures = options.Value.ForbiddenFeatures;

        var allFeatures = _extensionManager.GetFeatures();
        var featuresToDisable = allFeatures.Where(feature => forbiddenFeatures.Contains(feature.Id));
        await shellFeaturesManager.DisableFeaturesAsync(featuresToDisable);

        return;
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
