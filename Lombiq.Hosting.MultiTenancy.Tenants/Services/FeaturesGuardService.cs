using Lombiq.Hosting.MultiTenancy.Tenants.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions;
using OrchardCore.DisplayManagement;
using OrchardCore.Environment.Shell;
using System.Threading.Tasks;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Services;

public class FeaturesGuardService : IFeaturesGuardService
{
    private readonly RequestDelegate _next;
    private readonly IExtensionManager _extensionManager;
    private readonly OrchardCoreBuilder _orchardCoreBuilder;
    private readonly IShellFeaturesManager _shellFeaturesManager;

    public FeaturesGuardService(
        RequestDelegate next,
        IExtensionManager extensionManager,
        OrchardCoreBuilder orchardCoreBuilder,
        IShellFeaturesManager shellFeaturesManager)
    {
        _next = next;
        _extensionManager = extensionManager;
        _orchardCoreBuilder = orchardCoreBuilder;
        _shellFeaturesManager = shellFeaturesManager;
    }

    public Task InvokeAsync(
        HttpContext context,
        IOptions<ForbiddenFeaturesOptions> forbiddenOptions,
        IOptions<AlwaysOnFeaturesOptions> alwaysOnOptions)
    {

        DisableFeatures(context, forbiddenOptions);
        EnableFeatures(context, alwaysOnOptions);

        return _next(context);
    }

    public Task DisableFeatures(HttpContext context, IOptions<ForbiddenFeaturesOptions> options)
    {
        var forbiddenFeatures = options.Value.ForbiddenFeatures;

        var allFeatures = _extensionManager.GetFeatures();

        foreach (var feature in forbiddenFeatures)
        {
            // disable features 
        }

        return _next(context);
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
