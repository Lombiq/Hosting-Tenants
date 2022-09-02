using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Extensions.Features;
using System.Collections.Generic;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Services;

public class TenantFeatureBuilderEvents : IFeatureBuilderEvents
{
    private readonly IConfiguration _configuration;

    public TenantFeatureBuilderEvents(IHttpContextAccessor hca, IConfiguration configuration) =>
        _configuration = configuration;

    public void Building(FeatureBuildingContext context)
    {
        var configurationSections = _configuration
            .GetSection("OrchardCore:Lombiq_Hosting_MultiTenancy_Tenants:ForbiddenFeaturesOptions:ForbiddenFeatures")
            .GetChildren();

        var forbiddenFeatures = new List<string>();
        foreach (var section in configurationSections)
        {
            forbiddenFeatures.Add(section.Value);
        }

        if (forbiddenFeatures.Contains(context.FeatureId))
        {
            context.DefaultTenantOnly = true;
        }

        // for alwaysEnabledFeatures, set isAlwaysEnabled to true
        // needs to be run conditionally: only on user tenants (probably not possible from this method)
        //else
        //{
        //    var alwaysEnabledFeatures = _alwaysOnFeaturesOptions.Value.AlwaysOnFeatures;
        //    if (alwaysEnabledFeatures.Contains(context.FeatureId))
        //    {
        //        context.IsAlwaysEnabled = true;
        //    }
        //}
    }

    public void Built(IFeatureInfo featureInfo)
    {
        // Not needed.
    }
}
