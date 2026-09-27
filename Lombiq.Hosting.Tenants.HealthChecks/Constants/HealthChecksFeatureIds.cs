namespace Lombiq.Hosting.Tenants.HealthChecks.Constants;

public static class HealthChecksFeatureIds
{
    public const string Module = "Lombiq.Hosting.Tenants.HealthChecks";

    public const string Admin = $"{Module}.{nameof(Admin)}";
    public const string DefaultTenant = $"{Module}.{nameof(DefaultTenant)}";
    public const string AllTenants = $"{Module}.{nameof(AllTenants)}";
}
