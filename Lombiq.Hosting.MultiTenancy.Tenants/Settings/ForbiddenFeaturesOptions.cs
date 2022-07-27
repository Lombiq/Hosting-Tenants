using System.Collections.Generic;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Settings;

public class ForbiddenFeaturesOptions
{
    public IEnumerable<string> ForbiddenFeatureNames { get; set; }
}
