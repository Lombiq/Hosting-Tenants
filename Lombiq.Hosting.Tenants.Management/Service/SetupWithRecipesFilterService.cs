using Lombiq.Hosting.Tenants.Management.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.AutoSetup.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Setup.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Tenants.Management.Service
{
    public class SetupWithRecipesFilterService : ISetupService
    {
        private readonly SetupService _setupService;
        private readonly IOptions<HideRecipesFromSetupOptions> _hideRecipesFromSetupOptionsOptions;
        private readonly IOptions<AutoSetupOptions> _autoSetupOptionsOptions;
        private readonly ShellSettings _shellSettings;

#pragma warning disable S107 // Methods should not have too many parameters
        public SetupWithRecipesFilterService(
            IShellHost shellHost,
            IHostEnvironment hostEnvironment,
            IShellContextFactory shellContextFactory,
            ISetupUserIdGenerator setupUserIdGenerator,
            IEnumerable<IRecipeHarvester> recipeHarvesters,
            ILogger<SetupService> logger,
            IStringLocalizer<SetupService> stringLocalizer,
            IHostApplicationLifetime hostApplicationLifetime,
            IHttpContextAccessor hca,
            IOptions<HideRecipesFromSetupOptions> hideRecipesFromSetupOptionsOptions,
            IOptions<AutoSetupOptions> autoSetupOptionsOptions,
            ShellSettings shellSettings)
#pragma warning restore S107 // Methods should not have too many parameters
        {
            _setupService = new(
                shellHost,
                hostEnvironment,
                shellContextFactory,
                setupUserIdGenerator,
                recipeHarvesters,
                logger,
                stringLocalizer,
                hostApplicationLifetime,
                hca);

            _hideRecipesFromSetupOptionsOptions = hideRecipesFromSetupOptionsOptions;
            _autoSetupOptionsOptions = autoSetupOptionsOptions;
            _shellSettings = shellSettings;
        }

        public async Task<IEnumerable<RecipeDescriptor>> GetSetupRecipesAsync()
        {
            var recipesDescriptors = await _setupService.GetSetupRecipesAsync();
            var currentTenantSetupOptions = _autoSetupOptionsOptions.Value.Tenants.FirstOrDefault(tenant
                => tenant.ShellName == _shellSettings.Name);

            // The first case is when specify the tenant recepe name via default tenant admin UI,
            // the second case is when the tenant is already configured via AutoSetup feature.
            if (_shellSettings["RecipeName"] != null || currentTenantSetupOptions?.RecipeName != null)
            {
                return recipesDescriptors;
            }
            else
            {
                var hiddenCategories = _hideRecipesFromSetupOptionsOptions.Value.HiddenCategories;
                return recipesDescriptors.Where(recepe => !recepe.Categories.Any(category
                    => hiddenCategories.Contains(category)));
            }
        }

        public Task<string> SetupAsync(SetupContext context) => _setupService.SetupAsync(context);
    }
}
