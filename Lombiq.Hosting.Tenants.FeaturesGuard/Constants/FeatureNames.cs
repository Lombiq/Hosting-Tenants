namespace Lombiq.Hosting.Tenants.FeaturesGuard.Constants;

public static class FeatureNames
{
    public const string Module = "Lombiq.Hosting";

    public const string Tenants = Module + "." + nameof(Tenants);

    public const string FeaturesGuard = Tenants + "." + nameof(FeaturesGuard);
}
