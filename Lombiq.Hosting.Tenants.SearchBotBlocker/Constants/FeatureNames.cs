namespace Lombiq.Hosting.Tenants.SearchBotBlocker.Constants;

public static class FeatureNames
{
    public const string Module = "Lombiq.Hosting";

    public const string Tenants = Module + "." + nameof(Tenants);

    public const string SearchBotBlocker = Tenants + "." + nameof(SearchBotBlocker);
}
