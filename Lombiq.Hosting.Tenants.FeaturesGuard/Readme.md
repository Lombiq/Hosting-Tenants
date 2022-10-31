# Lombiq Hosting - Tenants Features Guard for Orchard Core

[![Lombiq.Hosting.Tenants.FeaturesGuard NuGet](https://img.shields.io/nuget/v/Lombiq.Hosting.Tenants.FeaturesGuard?label=Lombiq.Hosting.Tenants.FeaturesGuard)](https://www.nuget.org/packages/Lombiq.Hosting.Tenants.FeaturesGuard/)

## About

A module that makes it possible to conditionally enable and conditionally keep enabled, or entirely prevent enabling, configurable sets of features on tenants.

## Documentation

### Conditionally enabling features

- To use this feature, enable it on both the Default and the user tenant.
- Features that should be conditionally enabled, as well as the features whose status acts as the condition, can be specified in appsettings.json using `ConditionallyEnabledFeaturesOptions`.
Conditionally enabled features need to be provided with a singular or multiple condition features, where the status of the condition features determines whether the corresponding conditional feature is enabled or disabled. Example configuration:

```json
{
  "OrchardCore": {
    "Lombiq_Hosting_Tenants_FeaturesGuard": {
      "ConditionallyEnabledFeaturesOptions": {
        "ConditionallyEnabledFeatures": {
          "EnableFeatureIfOtherFeatureIsEnabled": {
            // "Enable this feature and keep it enabled": "If any of these features are enabled",
            "OrchardCore.Media.Azure.Storage": "OrchardCore.Media",
            "OrchardCore.Twitter": "OrchardCore.Media, OrchardCore.Workflows"
          }
        }
      }
    },
}
```

Example case 1:
- Enables `Azure Storage` when `Media` is enabled. Keeps `Azure Storage` enabled as long as `Media` is enabled.

Example case 2:
- Enables `Twitter` when `Media` or `Workflows` is enabled. Keeps `Twitter` enabled as long as either `Media` or `Workflows` remains enabled.

### Preventing enabling features

- Preventing enabling certain features on user tenants is possible via recipes in a `FeatureProfiles` step. For more on the Feature Profiles feature, see the [official documentation](https://docs.orchardcore.net/en/latest/docs/reference/modules/Tenants/#feature-profiles). Example configuration:

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
