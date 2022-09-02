using Lombiq.Hosting.MultiTenancy.Tenants.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Services;

public interface IFeaturesGuardService
{
    public Task EnableFeatures(HttpContext context, IOptions<AlwaysOnFeaturesOptions> options);
}
