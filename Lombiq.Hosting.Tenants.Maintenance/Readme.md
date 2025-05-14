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

### `Lombiq.Hosting.Tenants.Maintenance.AddAdministratorRoleToUsersWithRole`

A maintenance task that adds the `Administrator` role to users who have a role set in the app configuration. It is available on any tenant.

The following configuration options are available to set the role:

```json
{
  "OrchardCore": {
    "Lombiq_Hosting_Tenants_Maintenance": {
      "AddAdministratorRoleToUsersWithRole": {
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

Defining each tenant's URL separately is also an option, in this case, you have to use the `SiteUrlFromTenantName` property instead of `SiteUrl` and add your tenants' name and URL:

```json
{
  "OrchardCore": {
    "Lombiq_Hosting_Tenants_Maintenance": {
      "UpdateSiteUrl": {
        "IsEnabled": true,
        "DefaultTenantSiteUrl": "https://domain.com",
        "SiteUrlFromTenantName": {
          "Tenant1": "https://domain.com/custom-url",
          "Tenant2": "https://custom-domain.com"
        }
      }
    }
  }
}
```

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

It's a maintenance task that removes users from the database with the given email domain. It is available only for the default tenant. Useful if you have Azure AD enabled in your production environment and you want to reset staging to the production database. Then you would get "System.InvalidOperationException: Provider AzureAD is already linked for userName" error, so deleting those users will solve the error.

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

### `Lombiq.Hosting.Tenants.Maintenance.ChangeUserSensitiveContent`

It's a maintenance task that depersonalizes the user-names, e-mail addresses and passwords, so they are changed to realistic but random values. The maintenance task runs only on the tenants that are added to the `TenantNames` property.

The following configuration should be used to allow the maintenance to run:

```json
{
  "OrchardCore": {
    "Lombiq_Hosting_Tenants_Maintenance": {
      "ChangeUserSensitiveContent": {
        "IsEnabled": true,
        "TenantNames": "Default, Tenant1, Tenant2",
        "EmailExcludePattern": ".+@(lombiq.com|example.com|foo.com)$"
      }
    }
  }
}
```

Any user accounts with an e-mail matching the `EmailExcludePattern` regex will not be depersonalized.

### `Lombiq.Hosting.Tenants.Maintenance.DeleteElasticsearchIndices`

This contains a maintenance task that deletes all Elasticsearch indices related to the tenant that is being activated, and another one that rebuilds them.

It also contains a middleware that deletes all Elasticsearch indices related to the tenant, but it does that before the tenant setup. This is useful when you want to delete the indices before the tenant setup, so you can create indices from a recipe during setup. To be able to use the middleware before setup this feature must be added as a setup feature. You can do this with `OrchardCoreBuilder.AddSetupFeatures(Lombiq.Hosting.Tenants.Maintenance.Constants.FeatureNames.DeleteElasticsearchIndicesBeforeSetup);`. Please note that setup features are only enabled before setup and are disabled afterward.

The following configuration should be used to allow the maintenance to run and for the middleware to be added:

```json
{
  "OrchardCore": {
    "Lombiq_Hosting_Tenants_Maintenance": {
      "ElasticsearchIndicesOptions": {
          "DeleteMaintenanceIsEnabled": true,
          "RebuildMaintenanceIsEnabled": true,
          "BeforeSetupMiddlewareIsEnabled": true
      }
    }
  }
}
```

### `Lombiq.Hosting.Tenants.Maintenance.UpdateEnabledFeatures`

It's a maintenance task that updates the enabled features of a tenant based on the app configuration. It is available on any tenant.

The following configuration options are available to set the enabled features:

```json
{
  "OrchardCore": {
    "Lombiq_Hosting_Tenants_Maintenance": {
      "UpdateEnabledFeatures": {
        "IsEnabled": true,
        "EnableFeatures": "OrchardCore.Admin, OrchardCore.Alias, OrchardCore.AuditTrail",
        "DisableFeatures": "OrchardCore.BackgroundTasks, OrchardCore.ContentFields"
      }
    }
  }
}
```

### `Lombiq.Hosting.Tenants.Maintenance.StaggeredTenantWakeUp`

Adds a page to the admin under Multi-Tenancy/Staggered Tenant Wake Up that allows you to start all tenants shells in a staggered way. Waking up all the tenants this way ensures that migrations (and maintenance, should you have them) are all executed without having to start each tenant manually and also avoids saturating hardware resources (when configured correctly), such as the database access.

You can edit the maintenance options directly on the Multi-Tenancy / Staggered Tenant Wake Up page if you click on the edit button.

- Batch Size: The number of tenants processed in one iteration.
- Time Span Between Batches: The amount of time to wait between processing batches.
- Run In Parallel: Determines whether the number of tenants defined by the batch size are woken up in parallel or sequentially.
- RunOnStartup: When set to true, the staggered tenant wake-up maintenance will start a new version if a new deployment was done, or it will continue the current run if it hasn't finished yet. If you set this to false, you will have to start it manually from the admin page.

Functions on the page:

- Start new: Starts the staggered tenant wake-up maintenance with a new version number.
- Continue: Continues most recent if it was paused.
- Pause: Pauses the current run.
- Edit: Allows you to edit the options listed above. You can only save while the maintenance isn't running.

You can initialize the configuration with the following app settings:

```json
{
  "OrchardCore": {
    "Lombiq_Hosting_Tenants_Maintenance": {
      "StaggeredTenantWakeUp": {
        "BatchIntervalSeconds": 5,
        "BatchSize": 5,
        "RunParallel": false,
        "RunOnStartup": true
      }
    }
  }
}
```
