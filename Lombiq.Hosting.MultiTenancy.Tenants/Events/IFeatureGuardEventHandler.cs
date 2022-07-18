using Abp.Application.Features;
using Abp.Events.Bus.Handlers;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Events;
public interface IFeatureGuardEventHandler : IEventHandler
{
    void SetupDone();

    void ForbiddenFeatureEnablingAttempt(Feature feature);

    void PersistentFeatureDisablingAttempt(Feature feature);
}
