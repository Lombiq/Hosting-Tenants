namespace Lombiq.Hosting.MultiTenancy.Tenants.Constants;

public static class FeatureNames
{
    public const string Module = "Lombiq.Hosting.MultiTenancy.";

    public const string Tenants = Module + "." + nameof(Tenants);

    public const string OrchardCore = nameof(OrchardCore);

    public const string AzureStorage = OrchardCore + "." + "Media.Azure.Storage";
    public const string ContentTypes = OrchardCore + "." + "ContentTypes";
    public const string Liquid = OrchardCore + "." + "Liquid";
    public const string Media = OrchardCore + "." + "Media";
    public const string MediaCache = OrchardCore + "." + "Media.Cache";
    public const string Settings = OrchardCore + "." + "Settings";
    public const string Users = OrchardCore + "." + "Users";
}
