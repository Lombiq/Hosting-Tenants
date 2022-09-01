# Lombiq Hosting - Idle Tenant Management for Orchard Core

## About

With the help of this module, you can ensure that any tenant where the feature is enabled will shutdown after a preset time is elapsed. This can be used to free up resources.

## Documentation

This module contains one feature:

### `Lombiq.Hosting.Tenants.IdleTenantManagement`

This feature is available on any tenant. The maximum idle time can be set in the appsettings.json.

**NOTE:** Any tenant can have its own set of maximum idle time, however on default there is only one global time set.

```csharp
public void ConfigureServices(IServiceCollection services) =>
    services.AddOrchardCms(
        builder => builder.AddTenantFeatures(Lombiq.Hosting.Tenants.IdleTenantManagement.Constants.FeatureNames.DisableIdleTenants));
```

**NOTE:** This way the feature will also be enabled on the Default tenant. You may not want to use this feature on the default tenant. It's also available on all sites of [DotNest, the Orchard SaaS](https://dotnest.com/).
