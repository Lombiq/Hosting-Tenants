using System.Collections.Generic;
using static Lombiq.Hosting.Tenants.Management.Constants.DefaultValues;

namespace Lombiq.Hosting.Tenants.Management.Settings
{
    public class HideRecipesFromSetupOptions
    {
        public IEnumerable<string> HiddenTags { get; set; } = new[] { HideFromSetupScreen };
    }
}
