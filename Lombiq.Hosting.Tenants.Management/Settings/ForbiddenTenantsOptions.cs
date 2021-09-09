using System.Collections.Generic;

namespace Lombiq.Hosting.Tenants.Management.Settings
{
    public class ForbiddenTenantsOptions
    {
        public IEnumerable<string> RequestUrlHosts { get; set; }
    }
}
