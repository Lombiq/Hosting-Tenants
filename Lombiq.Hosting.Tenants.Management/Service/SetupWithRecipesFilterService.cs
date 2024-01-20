using Lombiq.Hosting.Tenants.Management.Settings;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;
using OrchardCore.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Management.Service;

public class SetupWithRecipesFilterService(
    IOptions<HideRecipesFromSetupOptions> hideRecipesFromSetupOptions,
    ShellSettings shellSettings,
    ISetupService setupService) : ISetupService
{
    public async Task<IEnumerable<RecipeDescriptor>> GetSetupRecipesAsync()
    {
        var recipesDescriptors = await setupService.GetSetupRecipesAsync();

        // The first case is when we specify the tenant recipe name via the Default tenant admin UI or AutoSetup
        // feature, the second case is necessary because the Default tenant doesn't fill in RecipeName even if we use
        // auto setup.
        if (shellSettings["RecipeName"] != null || shellSettings.Name.EqualsOrdinalIgnoreCase("Default"))
        {
            return recipesDescriptors;
        }

        var hiddenTags = hideRecipesFromSetupOptions.Value.HiddenTags;
        return recipesDescriptors.Where(recipe => !recipe.Tags.Exists(tag => hiddenTags.Contains(tag)));
    }

    public Task<string> SetupAsync(SetupContext context) => setupService.SetupAsync(context);
}
