# Lombiq Hosting Tenants FeaturesGuard for Orchard Core

[![Lombiq.Hosting.Tenants.FeaturesGuard NuGet](https://img.shields.io/nuget/v/Lombiq.Hosting.Tenants.FeaturesGuard?label=Lombiq.Hosting.Tenants.FeaturesGuard)](https://www.nuget.org/packages/Lombiq.Hosting.Tenants.FeaturesGuard/)

## About

This module contains the FeaturesGuard feature, which prevents disabling a configurable set of features on tenants.

## Documentation

- To use this feature, enable it on both the Default and the user tenant.
- Once enabled on the user tenant, the FeaturesGuard feature cannot be disabled.
- Features that should not be deactivatable can be specified in the appsettings JSON file using `AlwaysEnabledFeaturesOptions`. Example configuration:

```json
{
  "OrchardCore": {
    "Lombiq_Hosting_Tenants_FeaturesGuard": {
      "AlwaysEnabledFeaturesOptions": {
        "AlwaysEnabledFeatures": [
          "DotNest.Hosting.Tenants",
          "DotNest.TenantsAdmin.Subtenant",
          "Lombiq.Hosting.Tenants.Admin.Login",
          "OrchardCore.Roles",
          "OrchardCore.Users"
        ]
      }
    },
  }
}
```

- Additionally, the feature ensures whenever the OrchardCore.Media feature is enabled, the following features also get enabled:
  - OrchardCore.Content.Types
  - OrchardCore.Liquid
  - OrchardCore.Media.Azure.Storage
  - OrchardCore.Media.Cache
  - OrchardCore.Settings

- Preventing enabling certain features on user tenants is also possible via recipes in a FeatureProfiles step. Example configuration:

```json
{
  "name": "FeatureProfiles",
  "FeatureProfiles": {
    "Features Guard": {
      "FeatureRules": [
        {
            "Rule": "Exclude",
            "Expression": "OrchardCore.Workflows.Session"
        },
        {
            "Rule": "Exclude",
            "Expression": "OrchardCore.Lucene"
        },
        {
            "Rule": "Exclude",
            "Expression": "OrchardCore.MiniProfiler"
        },
        {
            "Rule": "Exclude",
            "Expression": "Lombiq.Tests.UI.Shortcuts"
        }
      ]
    }
  }
}
```

It's also available on all sites of [DotNest, the Orchard SaaS](https://dotnest.com/).

## Contributing and support

Bug reports, feature requests, comments, questions, code contributions and love letters are warmly welcome. You can send them to us via GitHub issues and pull requests. Please adhere to our [open-source guidelines](https://lombiq.com/open-source-guidelines) while doing so.

This project is developed by [Lombiq Technologies](https://lombiq.com/). Commercial-grade support is available through Lombiq.
