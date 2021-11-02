namespace Lombiq.Hosting.Tenants.Management.Constants
{
    public static class FeatureNames
    {
        public const string Module = "Lombiq.Hosting.Tenants.Management";

        public const string ForbiddenTenantNames = Module + "." + nameof(ForbiddenTenantNames);
        public const string HideRecipesFromSetup = Module + "." + nameof(HideRecipesFromSetup);
    }
}
