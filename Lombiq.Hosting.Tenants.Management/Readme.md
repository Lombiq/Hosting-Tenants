# Lombiq Hosting - Tenants Management



## About

With the help of this module, you can set restrictions on tenant creation.


## Documentation

This module contains two features:
- `Lombiq.Hosting.Tenants.Management.ForbiddenTenantNames`
- `Lombiq.Hosting.Tenants.Management.HideRecipesFromSetup`


### `Lombiq.Hosting.Tenants.Management.ForbiddenTenantNames`

With this module, you can specify a list of host names that cannot be used to create a tenant.  You can write the list of forbidden host names as a JSON array in the `appsettings.json` as follows:

```json
"OrchardCore": {
  "Lombiq_Hosting_Tenants_Management": {
    "Forbidden_Tenants_Options": {
      "RequestUrlHosts": [
        "forbidden.hostname1.net",
        "forbidden.hostname2.net"
      ]
    }
  }  
}
```

### `Lombiq.Hosting.Tenants.Management.HideRecipesFromSetup`

With this module, you can specify tags for recipes that won't be listed on the setup screen of tenants. Recipes with those tags will still be available from the Default tenant admin UI and they'll also be available to be used via the `AutoSetup` feature.

You can specify the recipe categories you want to hide using the `HideRecipesByCategoryFromSetup()` method in the web project's `ConfigureServices()`.

```csharp
public void ConfigureServices(IServiceCollection services) =>
    services.AddOrchardCms(builder => builder.HideRecipesByCategoryFromSetup("hiddenCategory1", "hiddenCategory2"))
```

## Dependencies 

This module has the following dependencies:

- [Lombiq Helpful Libraries for Orchard Core](https://github.com/Lombiq/Helpful-Libraries)
