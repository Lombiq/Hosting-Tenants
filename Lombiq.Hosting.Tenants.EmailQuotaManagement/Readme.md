# Lombiq Hosting - Tenants Media Storage Management for Orchard Core

[![Lombiq.Hosting.Tenants.EmailQuotaManagement NuGet](https://img.shields.io/nuget/v/Lombiq.Hosting.Tenants.EmailQuotaManagement?label=Lombiq.Hosting.Tenants.EmailQuotaManagement)](https://www.nuget.org/packages/Lombiq.Hosting.Tenants.EmailQuotaManagement/)

## About

With the help of this module, you can set restrictions regarding maximum email sent per tenant per month, if they are using the the same SMTP server host as the predefined one from the environment variables or from the `appsettings.json` file.

Do you want to quickly try out this project and see it in action? Check it out in our [Open-Source Orchard Core Extensions](https://github.com/Lombiq/Open-Source-Orchard-Core-Extensions) full Orchard Core solution and also see our other useful Orchard Core-related open-source projects!

## Documentation

This module currently contains one feature:

- `Lombiq.Hosting.Tenants.EmailQuotaManagement`

### `Lombiq.Hosting.Tenants.EmailQuotaManagement`

With this module, you can specify how much space would you like to limit each tenant's maximum email quota. The default is 1000 per month. You can change this value in the `appsettings.json` file or with an environment variable. When the quota is reached the email won't be sent and also the following will happen:

- An email will be sent to the tenant's users who has Site Owner permission.
- A warning message will be shown that the limit has been reached on the admin dashboard.

Also a warning message is always shown with the current email quota status on the email settings page when the same host is used as the predefined one from the environment variables or from the `appsettings.json` file.

```json
"OrchardCore": {
  "Lombiq_Hosting_Tenants_EmailQuotaManagement": {
    "EmailQuota": 42069
  }
}
```

Tenant based configuration can be defined as the following, for more details read the [Orchard Core documentation](https://docs.orchardcore.net/en/main/docs/reference/core/Configuration/#tenant-postconfiguration).

```json
"OrchardCore": {
  "TenantName": {
    "Lombiq_Hosting_Tenants_EmailQuotaManagement": {
      "EmailQuota": 42069
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
