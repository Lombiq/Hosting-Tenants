using System.Collections.Generic;

namespace Lombiq.Hosting.Tenants.FeaturesGuard.Models;

public class AlwaysEnabledFeaturesOptions
{
    public IEnumerable<string> AlwaysEnabledFeatures { get; set; }
}
