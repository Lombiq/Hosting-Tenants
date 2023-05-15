# Lombiq Hosting - Tenant Maintenance for Orchard Core

[![Lombiq.Hosting.Tenants.Maintenance NuGet](https://img.shields.io/nuget/v/Lombiq.Hosting.Tenants.Maintenance?label=Lombiq.Hosting.Tenants.Maintenance)](https://www.nuget.org/packages/Lombiq.Hosting.Tenants.Maintenance/)

## About

With the help of this module you can execute maintenance tasks on tenants. These tasks can be anything that you want to run on tenants, like updating the tenants' URL based on the app configuration.

## Documentation

Please see the below features for more information.

### `Lombiq.Hosting.Tenants.Maintenance`

This is the core functionality required to execute maintenance tasks on tenants. It is available on any tenant. To make your application execute maintenance tasks, you need to add the following to your `Startup.cs`:

```csharp
public void ConfigureServices(IServiceCollection services) =>
    services.AddOrchardCms(
        builder => builder.AddTenantFeatures(Lombiq.Hosting.Tenants.Maintenance.Constants.FeatureNames.Maintenance));
```

To add new maintenance tasks, you need to implement the `IMaintenanceProvider` interface and register it as a service.

### `Lombiq.Hosting.Tenants.Maintenance.TenantUrlMaintenanceCore`

Provides the core functionality for updating the tenants' URL based on the app configuration. It's a dependency of the `UpdateSiteUrl` and `UpdateShellRequestUrls` maintenance tasks and the configuration options affect them as well. The following configuration options are available to set the tenant URLs:

```json
{
  "OrchardCore": {
    "Lombiq_Hosting_Tenants_Maintenance": {
      "TenantUrlMaintenance": {
        "DefaultTenantUrl": "domain.com",
        "TenantUrl": "{TenantName}.domain.com"
      }
    }
  }
}
```

**NOTE**: The `{TenantName}` placeholder will be replaced with the actual tenant name automatically.

### `Lombiq.Hosting.Tenants.Maintenance.UpdateSiteUrl`

It's a maintenance task that updates the site's base URL in the site settings based on the app configuration. It is available on any tenant.

To make your application execute this task, you need to add the following to your `Startup.cs`:

```csharp
public void ConfigureServices(IServiceCollection services) =>
    services.AddOrchardCms(
        builder => builder.AddTenantFeatures(Lombiq.Hosting.Tenants.Maintenance.Constants.FeatureNames.UpdateTenantUrl));
```

The following configuration options are available to set the site URL:

```json
{
  "OrchardCore": {
    "Lombiq_Hosting_Tenants_Maintenance": {
      "UpdateSiteUrl": {
        "SiteUrl": "https://domain.com/{TenantName}"
      }
    }
  }
}
```

**NOTE**: The `{TenantName}` placeholder will be replaced with the actual tenant name automatically.

### `Lombiq.Hosting.Tenants.Maintenance.UpdateShellRequestUrls`

It's a maintenance task that updates the shell's request URLs in each tenant's shell settings based on the app configuration. It is available only for the default tenant.

The following configuration options are available to set the shell request URLs:

```json
{
  "OrchardCore": {
    "Lombiq_Hosting_Tenants_Maintenance": {
      "UpdateShellRequestUrl": {
        "DefaultRequestUrl": "domain.com",
        "RequestUrl": "{TenantName}.domain.com",
        "RequestUrlPrefix": "{TenantName}"
      }
    }
  }
}
```

**NOTE**: The `{TenantName}` placeholder will be replaced with the actual tenant name automatically.
