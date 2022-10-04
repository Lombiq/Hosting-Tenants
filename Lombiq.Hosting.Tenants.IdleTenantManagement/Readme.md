# Lombiq Hosting - Idle Tenant Management for Orchard Core

## About

With the help of this module, you can ensure that a tenant that didn't receive any requests for a configurable time will shut down. This can be used to free up resources and thus increase site density.

## Documentation

This module contains the feature below.

### `Lombiq.Hosting.Tenants.IdleTenantManagement`

This feature is available on any tenant. The maximum idle time can be set in the appsettings.json.

**NOTE:** Any tenant can have its own set of maximum idle time, however by default there is only one global time set.

```csharp
public void ConfigureServices(IServiceCollection services) =>
    services.AddOrchardCms(
        builder => builder.AddTenantFeatures(Lombiq.Hosting.Tenants.IdleTenantManagement.Constants.FeatureNames.ShutDownIdleTenants));
```

**NOTE:** This way the feature will also be enabled on the Default tenant. You may not want to use this feature on the default tenant, so configure the corresponding `MaxIdleMinutes` as `0`. You can do this in an _appsettings.json_ file like this (see [the docs](https://docs.orchardcore.net/en/latest/docs/reference/core/Configuration/) for more details):

```json
{
  "OrchardCore": {
    "Default": {
      "Lombiq_Hosting_Tenants_IdleTenantManagement": {
        "IdleShutdownOptions": {
          "MaxIdleMinutes": 0
        }
      }
    }
  }
}
```
