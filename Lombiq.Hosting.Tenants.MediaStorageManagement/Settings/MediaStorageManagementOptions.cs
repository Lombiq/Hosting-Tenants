namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Settings;

public class MediaStorageManagementOptions
{
    /// <summary>
    /// Gets or sets the maximum storage quota for a tenant in bytes. Default is 1GB.
    /// </summary>
    public long MaximumStorageQuota { get; set; }
}
