using System.Collections.Generic;

namespace Lombiq.Hosting.Tenants.FeaturesGuard.Models;

public class ConditionallyEnabledFeaturesOptions
{
    // Needs to be settable for binding.
#pragma warning disable CA2227 // Collection properties should be read only
    public IDictionary<string, IEnumerable<string>> EnableFeatureIfOtherFeatureIsEnabled { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
}
