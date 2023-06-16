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

### `Lombiq.Hosting.Tenants.Maintenance.AddSiteOwnerPermissionToRole`

It's a maintenance task that adds the `SiteOwner` permission to a role set in the app configuration. It is available on any tenant.

The following configuration options are available to set the role:

```json
{
  "OrchardCore": {
    "Lombiq_Hosting_Tenants_Maintenance": {
      "AddSiteOwnerPermissionToRole": {
        "IsEnabled": true,
        "RoleName": "NameOfTheRole"
      }
    }
  }
}
```

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
        "IsEnabled": true,
        "SiteUrl": "https://domain.com/{TenantName}",
        "DefaultTenantSiteUrl": "https://domain.com"
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
        "IsEnabled": true,
        "DefaultShellRequestUrl": "domain.com",
        "RequestUrl": "{TenantName}.domain.com",
        "DefaultShellRequestUrlPrefix": "",
        "RequestUrlPrefix": "{TenantName}"
      }
    }
  }
}
```

**NOTE**: The `{TenantName}` placeholder will be replaced with the actual tenant name automatically.

### `Lombiq.Hosting.Tenants.Maintenance.RemoveUsers`

It's a maintenance task that removes users from the database with the given email domain. It is available only for the default tenant. Useful if you have Azure AD enabled in your production environment and you want to reset staging to the production database. Then you would get "System.InvalidOperationException: Provider AzureAD is already linked for userName" error, so deleting those users.

The following configuration should be used to allow the maintenance to run:

```json
{
  "OrchardCore": {
    "Lombiq_Hosting_Tenants_Maintenance": {
      "RemoveUsers": {
        "IsEnabled": true,
        "EmailDomain": "example.com"
      }
    }
  }
}
```
