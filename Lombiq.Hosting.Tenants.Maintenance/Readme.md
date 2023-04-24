# Lombiq Hosting - Tenant Maintenance for Orchard Core

## About

With the help of this module you can execute maintenance tasks on tenants.

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

### `Lombiq.Hosting.Tenants.Maintenance.UpdateTenantUrl`

It's a maintenance task that updates the tenants' URL based on the app configuration. It is available on any tenant. To make your application execute this task, you need to add the following to your `Startup.cs`:

```csharp
public void ConfigureServices(IServiceCollection services) =>
    services.AddOrchardCms(
        builder => builder.AddTenantFeatures(Lombiq.Hosting.Tenants.Maintenance.Constants.FeatureNames.UpdateTenantUrl));
```~~~~

To configure the task, you need to add the following to your `appsettings.json`:

```json
{
  "OrchardCore": {
    "Lombiq_Hosting_Tenants_Maintenance": {
      "UpdateTenantUrl": {
        "Enabled": true,
        "DefaultTenantUrl": "domain.com"
        "TenantUrl": "{TenantName}.domain.com"
      }
    },
  }
}
```

**NOTE**: The `{TenantName}` placeholder will be replaced with the actual tenant name.~~~~