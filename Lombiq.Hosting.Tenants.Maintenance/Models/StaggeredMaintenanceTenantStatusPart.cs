using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public class StaggeredMaintenanceTenantStatusPart : ContentPart
{
    public IDictionary<string, string> Versions { get; } = new Dictionary<string, string>();
}
