using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public class StaggeredMaintenancePart : ContentPart
{
    public NumericField ProgressPercentage { get; set; } = new() { Value = 0 };
    public NumericField AllTenantCount { get; set; } = new() { Value = 0 };
    public NumericField ProcessedTenantsCount { get; set; } = new() { Value = 0 };
    public NumericField ProcessingStep { get; set; } = new() { Value = 1 };
    public NumericField CurrentVersion { get; set; } = new() { Value = 0 };
    public IList<string> ProcessedTenantIds { get; } = [];

    public HtmlField ErrorLogsHtmlField { get; set; } = new()
    {
        Html = JsonSerializer.Serialize(new Dictionary<string, string>(), JOptions.CamelCaseIndented),
    };

    [JsonIgnore]
    public IDictionary<string, string> ErrorLogs { get; } = new Dictionary<string, string>();

    public void AddErrorLog(string tenantId, string error)
    {
        ErrorLogs.Add(tenantId, error);
        ErrorLogsHtmlField.Html = JsonSerializer.Serialize(ErrorLogs, JOptions.CamelCaseIndented);
    }

    public void Clear()
    {
        ProcessedTenantIds.Clear();
        AllTenantCount.Value = 0;
        ProcessedTenantsCount.Value = 0;
        ProgressPercentage.Value = 0;
        ErrorLogs.Clear();
        ErrorLogsHtmlField.Html = JsonSerializer.Serialize(ErrorLogs, JOptions.CamelCaseIndented);
    }
}
