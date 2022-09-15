# Lombiq Hosting MultiTenancy Tenants


## About

This module contains the FeaturesGuard feature, which prevents disabling a configurable set of features on tenants.


## Documentation

- To use this feature, enable it on both the Default and the user tenant.
- Once enabled on the user tenant, the FeaturesGuard feature cannot be disabled.
- Features that should not be deactivatable can be specified in the appsettings JSON file using `AlwaysEnabledFeaturesOptions`.
- Additionally, the feature ensures whenever the OrchardCore.Media feature is enabled, the following features also get enabled:
	- OrchardCore.Content.Types
	- OrchardCore.Liquid
	- OrchardCore.Media.Azure.Storage
	- OrchardCore.Media.Cache
	- OrchardCore.Settings


## Contributing and support

Bug reports, feature requests, comments, questions, code contributions, and love letters are warmly welcome, please do so via GitHub issues and pull requests. Please adhere to our [open-source guidelines](https://lombiq.com/open-source-guidelines) while doing so.

This project is developed by [Lombiq Technologies](https://lombiq.com/). Commercial-grade support is available through Lombiq.
