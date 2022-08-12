using System.Collections.Generic;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Models;
public class ForbiddenFeaturesOptions
{
    public IEnumerable<string> ForbiddenFeatures { get; set; }
}
