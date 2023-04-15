using Lombiq.Hosting.Tenants.MediaStorageManagement.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;

namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Settings;

public class UploadFileLimitOptions : IConfigureOptions<MvcOptions>
{
    private readonly IServiceProvider _serviceProvider;

    public UploadFileLimitOptions(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public void Configure(MvcOptions options)
        => options.Conventions.Add(new DynamicMediaSizeApplicationModelConvention(_serviceProvider));
}
