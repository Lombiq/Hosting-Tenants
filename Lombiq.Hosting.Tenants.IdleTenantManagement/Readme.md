# Lombiq Hosting - Idle Tenant Management for Orchard Core

## About

With the help of this module, you can ensure that a tenant that didn't receive any requests for a configurable time will shut down. This can be used to free up resources and thus increase site density.

## Documentation

This module contains one feature: 

### `Lombiq.Hosting.Tenants.IdleTenantManagement`

This feature is available on any tenant. The maximum idle time can be set in the appsettings.json.

**NOTE:** Any tenant can have its own set of maximum idle time, however by default there is only one global time set.

```csharp
public void ConfigureServices(IServiceCollection services) =>
    services.AddOrchardCms(
        builder => builder.AddTenantFeatures(Lombiq.Hosting.Tenants.IdleTenantManagement.Constants.FeatureNames.DisableIdleTenants));
```

**NOTE:** This way the feature will also be enabled on the Default tenant. You may not want to use this feature on the default tenant. It's also available on all sites of [DotNest, the Orchard SaaS](https://dotnest.com/).
