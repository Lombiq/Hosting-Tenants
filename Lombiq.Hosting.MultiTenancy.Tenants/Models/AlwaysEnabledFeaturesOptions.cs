using System.Collections.Generic;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Models;

public class AlwaysEnabledFeaturesOptions
{
    public IEnumerable<string> AlwaysEnabledFeatures { get; set; }
}
