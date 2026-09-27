namespace Lombiq.Hosting.Tenants.HealthChecks.Constants;

public static class ConfigurationKeys
{
    public const string ConfigurationKey = "OrchardCore:Lombiq_Hosting_Tenants_HealthChecks";
    public const string IsEnabled = $"{ConfigurationKey}:{nameof(IsEnabled)}";
}
