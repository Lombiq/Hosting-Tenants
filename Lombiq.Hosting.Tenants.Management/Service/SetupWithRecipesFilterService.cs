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

public class SetupWithRecipesFilterService : ISetupService
{
    private readonly IOptions<HideRecipesFromSetupOptions> _hideRecipesFromSetupOptions;
    private readonly ShellSettings _shellSettings;
    private readonly ISetupService _setupService;

    public SetupWithRecipesFilterService(
        IOptions<HideRecipesFromSetupOptions> hideRecipesFromSetupOptions,
        ShellSettings shellSettings,
        ISetupService setupService)
    {
        _hideRecipesFromSetupOptions = hideRecipesFromSetupOptions;
        _shellSettings = shellSettings;
        _setupService = setupService;
    }

    public async Task<IEnumerable<RecipeDescriptor>> GetSetupRecipesAsync()
    {
        var recipesDescriptors = await _setupService.GetSetupRecipesAsync();

        // The first case is when we specify the tenant recipe name via the Default tenant admin UI or AutoSetup
        // feature, the second case is necessary because the Default tenant doesn't fill in RecipeName even if we use
        // auto setup.
        if (_shellSettings["RecipeName"] != null || _shellSettings.Name.EqualsOrdinalIgnoreCase("Default"))
        {
            return recipesDescriptors;
        }

        var hiddenTags = _hideRecipesFromSetupOptions.Value.HiddenTags;
        return recipesDescriptors.Where(recipe => !recipe.Tags.Exists(tag => hiddenTags.Contains(tag)));
    }

    public Task<string> SetupAsync(SetupContext context) => _setupService.SetupAsync(context);
}
