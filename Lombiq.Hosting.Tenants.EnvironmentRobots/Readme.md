# Lombiq Hosting - Tenants Environment Robots for Orchard Core

[![Lombiq.Hosting.Tenants.EnvironmentRobots NuGet](https://img.shields.io/nuget/v/Lombiq.Hosting.Tenants.EnvironmentRobots?label=Lombiq.Hosting.Tenants.EnvironmentRobots)](https://www.nuget.org/packages/Lombiq.Hosting.Tenants.EnvironmentRobots/)

## About

A module that prevents search bots from indexing non-production environments.

## Documentation

This module contains the feature below.

### `Lombiq.Hosting.Tenants.EnvironmentRobots`

The module works by adding a `X-Robots-Tag` header with the value `noindex`, `nofollow` to the HTTP response of non-production apps. This instructs search engines not to index or follow the links on these pages. Additionally, the module also adds a `<meta name="robots" content="noindex, nofollow">` tag to the HTML head of non-production apps for the same purpose.

The module determines whether a app is non-production based on the `IsProduction` option in the `EnvironmentRobotsOptions` class. By default, this option is set to `null`, which means the module will use the `IHostEnvironment.IsProduction()` method to check the environment name. However, you can override this option by setting it to `true` or `false` in the `appsettings.json` file under the `Lombiq_Hosting_Tenants_EnvironmentRobots:EnvironmentRobotsOptions:IsProduction` section as follows:
```json
"OrchardCore": {
  "Lombiq_Hosting_Tenants_EnvironmentRobots": {
    "EnvironmentRobotsOptions": {
      "IsProduction": true
    }
  }  
}
```

To use the module, follow these steps:

- Enable the `Lombiq.Hosting.Tenants.EnvironmentRobots` feature from the admin dashboard.
- Optionally, configure the `IsProduction` option for each tenant as described above.


