using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public class StaggeredMaintenanceTenantStatusPart : ContentPart
{
    public NumericField Version { get; set; } = new();
}
