# Lombiq Hosting - Tenants Management



## About

With the help of this module, you can set restrictions on tenant creation.


## Documentation

With this module, you can specify a list of host names that cannot be used to create a tenant.  You can write the list of forbidden host names as a JSON array in the `appsettings.json` as follows:

```json
"OrchardCore": {
    "Lombiq_Hosting_Tenants_Management": {
      "Forbidden_Tenants_Options": {
        "RequestUrlHosts": [
          "forbidden.hostname1.net",
          "forbidden.hostname2.net"
        ]
      }
    }  
}
```


## Dependencies 

This module has the following dependencies:

- [Lombiq Helpful Libraries for Orchard Core](https://github.com/Lombiq/Helpful-Libraries)