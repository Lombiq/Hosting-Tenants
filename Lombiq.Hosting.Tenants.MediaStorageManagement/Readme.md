# Lombiq Hosting - Tenants Media Storage Management for Orchard Core

[![Lombiq.Hosting.Tenants.MediaStorageManagement NuGet](https://img.shields.io/nuget/v/Lombiq.Hosting.Tenants.MediaStorageManagement?label=Lombiq.Hosting.Tenants.MediaStorageManagement)](https://www.nuget.org/packages/Lombiq.Hosting.Tenants.MediaStorageManagement/)

## About

With the help of this module, you can set restrictions regarding maximum media space per tenant.

Do you want to quickly try out this project and see it in action? Check it out in our [Open-Source Orchard Core Extensions](https://github.com/Lombiq/Open-Source-Orchard-Core-Extensions) full Orchard Core solution and also see our other useful Orchard Core-related open-source projects!

## Documentation

This module currently contains one feature:

- `Lombiq.Hosting.Tenants.MediaStorageManagement`

### `Lombiq.Hosting.Tenants.MediaStorageManagement`

With this module, you can specify how much space would you like to limit each tenant's storage space. The default is 1GB. If you want to set it to 2GB e.g. you can do it in bytes as an environment variable or in _appsettings.json_ as follows:

```json
"OrchardCore": {
  "Lombiq_Hosting_Tenants_MediaStorageManagement": {
    "MaximumStorageQuotaBytes": 2147483648
  }  
}
```

Tenant based configuration can be defined as the following, for more details read the [Orchard Core documentation](https://docs.orchardcore.net/en/main/docs/reference/core/Configuration/#tenant-postconfiguration).

```json
"OrchardCore": {
  "TenantName": {
    "Lombiq_Hosting_Tenants_MediaStorageManagement": {
      "MaximumStorageQuotaBytes": 2147483648
    }
  }
}
```
