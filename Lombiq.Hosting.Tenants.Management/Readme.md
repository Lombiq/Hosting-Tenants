# Lombiq Hosting - Tenants Management for Orchard Core

[![Lombiq.Hosting.Tenants.Management NuGet](https://img.shields.io/nuget/v/Lombiq.Hosting.Tenants.Management?label=Lombiq.Hosting.Tenants.Management)](https://www.nuget.org/packages/Lombiq.Hosting.Tenants.Management/)

## About

With the help of this module, you can set restrictions on tenant creation.

## Documentation

This module contains two features:

- `Lombiq.Hosting.Tenants.Management.ForbiddenTenantNames`
- `Lombiq.Hosting.Tenants.Management.HideRecipesFromSetup`

### `Lombiq.Hosting.Tenants.Management.ForbiddenTenantNames`

With this module, you can specify a list of host names that cannot be used to create a tenant. You can write the list of forbidden host names as a JSON array in the `appsettings.json` as follows:

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

By default you can use `"HiddenFromSetupScreen"` tag on the recipe to hide it or you can specify the recipe tags you want to hide using the `HideRecipesByTagsFromSetup()` method in the web project's `ConfigureServices()`.

```csharp
public void ConfigureServices(IServiceCollection services) =>
    services.AddOrchardCms(builder => builder.HideRecipesByTagsFromSetup("hiddenTag1", "hiddenTag2"))
```

**NOTE:** This extension method not only sets the tags you want to hide but also registers the feature as a setup feature. If you just want to use the default hide tag you can call the extension function without any parameter.

## Dependencies

This module has the following dependencies:

- [Lombiq Helpful Libraries for Orchard Core](https://github.com/Lombiq/Helpful-Libraries)
