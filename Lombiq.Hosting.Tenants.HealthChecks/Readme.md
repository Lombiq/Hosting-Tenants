# Lombiq Hosting - Tenants Health Checks for Orchard Core

[![Lombiq.Hosting.Tenants.Lombiq.Hosting.Tenants.HealthChecks NuGet](https://img.shields.io/nuget/v/Lombiq.Hosting.Tenants.Lombiq.Hosting.Tenants.HealthChecks?label=Lombiq.Hosting.Tenants.Lombiq.Hosting.Tenants.HealthChecks)](https://www.nuget.org/packages/Lombiq.Hosting.Tenants.Lombiq.Hosting.Tenants.HealthChecks/)

## About

A module adds features specific to Orchard Core's [Health Checks module](https://docs.orchardcore.net/en/main/reference/modules/HealthChecks/).

## Documentation

### Admin UI

By going to Admin > Multi-tenancy > Health Checks on the default tenant, you can see the list of unhealthy tenants.

### Health Check Logging and Propagation

If a tenant is detected to be non-healthy, it's logged by the Default tenant. In turn this marks the default tenant as unhealthy as well.

### Periodic Self Health Checks

The `HealthCheckService.CheckHealthAsync` is periodically triggered from a background task.
