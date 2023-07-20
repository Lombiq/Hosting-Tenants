# Lombiq Hosting - Tenants Media Storage Management for Orchard Core

[![Lombiq.Hosting.Tenants.MediaStorageManagement NuGet](https://img.shields.io/nuget/v/Lombiq.Hosting.Tenants.MediaStorageManagement?label=Lombiq.Hosting.Tenants.MediaStorageManagement)](https://www.nuget.org/packages/Lombiq.Hosting.Tenants.MediaStorageManagement/)

## About

With the help of this module, you can set restrictions regarding maximum media space per tenant.

Do you want to quickly try out this project and see it in action? Check it out in our [Open-Source Orchard Core Extensions](https://github.com/Lombiq/Open-Source-Orchard-Core-Extensions) full Orchard Core solution and also see our other useful Orchard Core-related open-source projects!

## Documentation

This module currently contains one feature:

- `Lombiq.Hosting.Tenants.MediaStorageManagement`

### `Lombiq.Hosting.Tenants.MediaStorageManagement`

With this module, you can specify how much space would you like to limit each tenant's storage space. The default is 1GB. If you want to set it to 2GB e.g. you can do it in bytes as an environment variable or in `appsettings.json` as follows:

```json
"OrchardCore": {
  "Lombiq_Hosting_Tenants_MediaStorageManagement": {
    "Media_Storage_Management_Options": {
      "MaximumStorageQuotaBytes": 2147483648
    }
  }  
}
```

Tenant based configuration can be defined as the following, for more details read the [Orchard Core documentation](https://docs.orchardcore.net/en/main/docs/reference/core/Configuration/#tenant-postconfiguration).

```json
"OrchardCore": {
  "TenantName": {
    "Lombiq_Hosting_Tenants_MediaStorageManagement": {
      "Media_Storage_Management_Options": {
        "MaximumStorageQuotaBytes": 2147483648
      }
    }
  }
}
```

## Dependencies

This module has the following dependencies:

- [Lombiq Helpful Libraries for Orchard Core](https://github.com/Lombiq/Helpful-Libraries)

## Contributing and support

Bug reports, feature requests, comments, questions, code contributions and love letters are warmly welcome. You can send them to us via GitHub issues and pull requests. Please adhere to our [open-source guidelines](https://lombiq.com/open-source-guidelines) while doing so.

This project is developed by [Lombiq Technologies](https://lombiq.com/). Commercial-grade support is available through Lombiq.
