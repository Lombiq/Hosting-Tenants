namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Constants;

public static class FeatureNames
{
    private const string Module = "Lombiq.Hosting";

    public const string Tenants = Module + "." + nameof(Tenants);

    public const string EmailQuotaManagement = Tenants + "." + nameof(EmailQuotaManagement);
}
