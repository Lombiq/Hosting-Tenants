using IoC;
using OrchardCore.Environment.Extensions.Features;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Services;
public class TenantConfigurationCollector : ITenantConfigurationCollector
{
    private static readonly IEnumerable<string> _defaultPersistentFeatures = new[]
        {
            "Lombiq.Hosting.MultiTenancy.Tenants"
        };

    // It makes no sense to enable these modules on tenants.
    private static readonly IEnumerable<string> _defaultForbiddenFeatures = new[]
    {
            "DatabaseUpdate",
            "Lombiq.Hosting.Extensions",
            "Lombiq.Hosting.ShellManagement",
            "Lombiq.Hosting.MultiTenancy",
            "Lombiq.Hosting.MultiTenancy.Api",
            "Lombiq.Hosting.MultiTenancy.Maintenance",
            "Orchard.CodeGeneration",
            "Orchard.Migrations",
            "Orchard.MultiTenancy",
            "Gallery",
            "Gallery.Updates",
            "PackagingServices",
            "Orchard.Packaging",
            "Orchard.Redis",
            "Orchard.Media",
            "Orchard.MediaPicker",
            "Orchard.MessageBus",
            "Orchard.Rules"
        };

    private readonly IEnumerable<ITenantConfigurator> _configurators;

    public TenantConfigurationCollector(IEnumerable<ITenantConfigurator> configurators)
    {
        _configurators = configurators;
    }

    public ITenantConfiguration CollectConfiguration()
    {
        var configuration = new Configuration
        {
            PersistentFeatures = new List<IFeatureInfo>(_defaultPersistentFeatures),
            ForbiddenFeatures = new List<IFeatureInfo>(_defaultForbiddenFeatures)
        };

        foreach (var configurator in _configurators)
        {
            configurator.Configure(configuration);
        }

        return configuration;
    }

    public bool TryBuildExpression([NotNull] IBuildContext buildContext, [CanBeNull] ILifetime lifetime, out Expression expression, out Exception error) => throw new NotImplementedException();

    private class Configuration : ITenantConfiguration
    {
        IList<IFeatureInfo> ITenantConfiguration.PersistentFeatures { get; set; }

        IList<IFeatureInfo> ITenantConfiguration.ForbiddenFeatures { get; set; }
    }
}
