using Lombiq.Hosting.Tenants.Management.Settings;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Management.Service
{
    public class SetupWithRecipesFilterService : ISetupService
    {
        private readonly IOptions<HideRecipesFromSetupOptions> _hideRecipesFromSetupOptionsOptions;
        private readonly ShellSettings _shellSettings;
        private readonly ISetupService _setupService;

        public SetupWithRecipesFilterService(
            IOptions<HideRecipesFromSetupOptions> hideRecipesFromSetupOptionsOptions,
            ShellSettings shellSettings,
            ISetupService setupService)
        {
            _hideRecipesFromSetupOptionsOptions = hideRecipesFromSetupOptionsOptions;
            _shellSettings = shellSettings;
            _setupService = setupService;
        }

        public async Task<IEnumerable<RecipeDescriptor>> GetSetupRecipesAsync()
        {
            var recipesDescriptors = await _setupService.GetSetupRecipesAsync();

            // The first case is when specify the tenant recipe name via the Default tenant admin UI or AutoSetup
            // feature, the second case is necessary because the default tenant doesn't fill in RecepeName even if we use
            // auto setup.
            if (_shellSettings["RecipeName"] != null || _shellSettings.Name.EqualsOrdinalIgnoreCase("Default"))
            {
                return recipesDescriptors;
            }
            else
            {
                var hiddenCategories = _hideRecipesFromSetupOptionsOptions.Value.HiddenCategories;
                return recipesDescriptors
                    .Where(recipe => !recipe.Categories.Any(category => hiddenCategories.Contains(category)));
            }
        }

        public Task<string> SetupAsync(SetupContext context) => _setupService.SetupAsync(context);
    }
}
