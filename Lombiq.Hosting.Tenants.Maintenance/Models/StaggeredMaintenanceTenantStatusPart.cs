using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lombiq.Hosting.Tenants.Maintenance.Models;

public class StaggeredMaintenanceTenantStatusPart : ContentPart
{
    public HtmlField VersionsHtmlField { get; set; } = new()
    {
        Html = JsonSerializer.Serialize(new Dictionary<string, string>()),
    };

    [JsonIgnore]
    public IDictionary<string, string> Versions { get; private set; } = new Dictionary<string, string>();

    public void AddVersion(string tenantId, string version)
    {
        if (!Versions.Any())
        {
            Versions = JsonSerializer.Deserialize<IDictionary<string, string>>(VersionsHtmlField.Html);
        }

        Versions[tenantId] = version;
        VersionsHtmlField.Html = JsonSerializer.Serialize(Versions, JOptions.CamelCaseIndented);
    }
}
