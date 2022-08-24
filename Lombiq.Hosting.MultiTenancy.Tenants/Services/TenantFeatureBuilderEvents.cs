using OrchardCore.Environment.Extensions.Features;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Services;

public class TenantFeatureBuilderEvents : FeatureBuilderEvents
{
    public override void Building(FeatureBuildingContext context)
    {
        if (context.FeatureId != "OrchardCore.Lucene") return;

        context.DefaultTenantOnly = true;
    }

    //public void Built(IFeatureInfo featureInfo)
    //{
    //    // Not needed.
    //}
}
