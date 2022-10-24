using System.Collections.Generic;

namespace Lombiq.Hosting.Tenants.FeaturesGuard.Models;

public class ConditionallyEnabledFeaturesOptions
{
    public IDictionary<string, string> EnableFeatureIfOtherFeatureIsEnabled { get; set; }
}
