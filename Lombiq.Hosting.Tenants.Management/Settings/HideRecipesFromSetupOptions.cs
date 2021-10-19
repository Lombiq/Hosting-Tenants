using System.Collections.Generic;

namespace Lombiq.Hosting.Tenants.Management.Settings
{
    public class HideRecipesFromSetupOptions
    {
        public IEnumerable<string> HiddenCategories { get; set; }
    }
}
