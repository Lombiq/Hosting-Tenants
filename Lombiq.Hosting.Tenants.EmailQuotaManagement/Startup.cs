using Lombiq.Hosting.Tenants.EmailQuotaManagement.Constants;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Email;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using static Lombiq.Hosting.Tenants.EmailQuotaManagement.Constants.EmailQuotaOptionsConstants;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement;

[Feature(FeatureNames.EmailQuotaManagement)]
public class Startup : StartupBase
{
}
