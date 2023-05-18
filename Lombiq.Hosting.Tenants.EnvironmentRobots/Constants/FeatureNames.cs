namespace Lombiq.Hosting.Tenants.EnvironmentRobots.Constants;

public static class FeatureNames
{
    public const string Module = "Lombiq.Hosting";

    public const string Tenants = Module + "." + nameof(Tenants);

    public const string EnvironmentRobots = Tenants + "." + nameof(EnvironmentRobots);
}
