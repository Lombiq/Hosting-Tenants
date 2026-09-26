# Lombiq Hosting - Tenants Health Checks for Orchard Core

[![Lombiq.Hosting.Tenants.Lombiq.Hosting.Tenants.HealthChecks NuGet](https://img.shields.io/nuget/v/Lombiq.Hosting.Tenants.Lombiq.Hosting.Tenants.HealthChecks?label=Lombiq.Hosting.Tenants.Lombiq.Hosting.Tenants.HealthChecks)](https://www.nuget.org/packages/Lombiq.Hosting.Tenants.Lombiq.Hosting.Tenants.HealthChecks/)

## About

A module that adds tenant-aware features specific to Orchard Core's [Health Checks module](https://docs.orchardcore.net/en/main/reference/modules/HealthChecks/).

## Usage

Some of this module's features depend on being always-enabled. You can easily achieve that using the `AddTenantFeatures` and `AddDefaultTenantFeatures` extension methods inside the `AddOrchardCms` call of your _Program.cs_ file, like this:

```csharp
builder.Services.AddOrchardCms(orchardCoreBuilder => orchardCoreBuilder
    .AddTenantFeatures(HealthChecksFeatureIds.AllTenants)
    .AddDefaultTenantFeatures(HealthChecksFeatureIds.DefaultTenant)
);
```

## Documentation

### Admin UI

By going to Admin > Multi-tenancy > Health Checks on the Default tenant, you can see the list of unhealthy tenants.

### Health Check Logging and Propagation

If a tenant is detected to be non-healthy, it's logged by the Default tenant. In turn this marks the Default tenant as unhealthy as well.

### Periodic Self Health Checks

The `HealthCheckService.CheckHealthAsync` is periodically triggered from a background task.
