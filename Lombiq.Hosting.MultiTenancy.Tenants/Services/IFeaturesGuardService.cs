using Lombiq.Hosting.MultiTenancy.Tenants.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using System.Threading.Tasks;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Services;

public interface IFeaturesGuardService
{
    public Task DisableFeatures(
        HttpContext context,
        IOptions<ForbiddenFeaturesOptions> options,
        IShellFeaturesManager shellFeaturesManager);

    public Task EnableFeatures(HttpContext context, IOptions<AlwaysOnFeaturesOptions> options);
}
