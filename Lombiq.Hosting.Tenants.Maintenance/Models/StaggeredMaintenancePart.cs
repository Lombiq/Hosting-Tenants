using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public class StaggeredMaintenancePart : ContentPart
{
    public NumericField ProgressPercentage { get; set; } = new() { Value = 0 };
    public NumericField AllTenantCount { get; set; } = new() { Value = 0 };
    public NumericField ProcessedTenantsCount { get; set; } = new() { Value = 0 };
    public NumericField CurrentVersion { get; set; } = new() { Value = 0 };

    [JsonIgnore]
    public IList<string> ProcessedTenantNames { get; } = [];

    [JsonIgnore]
    public IDictionary<string, string> ErrorLogs { get; } = new Dictionary<string, string>();
}
