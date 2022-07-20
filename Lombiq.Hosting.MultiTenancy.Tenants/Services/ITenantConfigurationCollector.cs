using IoC;
using System.Collections.Generic;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Services;
public interface ITenantConfiguration
{
    IList<OrchardCore.Environment.Extensions.Features.IFeatureInfo> PersistentFeatures { get; }
    IList<OrchardCore.Environment.Extensions.Features.IFeatureInfo> ForbiddenFeatures { get; }
}

public interface ITenantConfigurator : IDependency
{

}
public interface ITenantConfigurationCollector : IDependency
{
    ITenantConfiguration CollectConfiguration();
}
