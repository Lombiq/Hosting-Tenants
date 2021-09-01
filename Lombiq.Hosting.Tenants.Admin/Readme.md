# Lombiq Hosting Tenants - Admin

## About

With the help of this module, you can log in from the Default tenant admin dashboard to any other tenants as an administrator user.

## Documentation

This module contains two features:
- Lombiq.Hosting.Tenants.Admin
- Lombiq.Hosting.Tenants.Admin.SubTenant

### Lombiq.Hosting.Tenants.Admin

This feature is only available on the Default tenant. It provides a button on the tenant edit page. After clicking this button the site login to the selected tenant as an administrator and redirect to the tenant's admin dashboard. 

**_NOTE:_**  Login will not be successful if the tenant has no user with administrator role

The feature also provides a LoginAsAdmin permission. Only the user who has this permission can see the login button.

### Lombiq.Hosting.Tenants.Admin.SubTenant

This feature provides a controller which can authenticate the request from the Default tenant as an administrator user. From the Default tenant, you can log in to only those tenants in which this feature is enabled.

## How to enable SubTenant feature on every tenants

In the web project's Startup class in the following way, you can enable Lombiq.Hosting.Tenants.Admin.SubTenant feature on all tenants.

```csharp
public void ConfigureServices(IServiceCollection services) =>
            services.AddOrchardCms(
                builder =>
                {
                    builder.AddTenantFeatures("Lombiq.Hosting.Tenants.Admin.SubTenant");
                });
```

**_NOTE:_**  This way the feature will also be enabled in the Default tenant. Therefore, for greater security, the controller does not perform authentication on the Default tenant.


 
