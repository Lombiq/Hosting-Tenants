namespace Lombiq.Hosting.Tenants.MediaStorageManagement.Settings;

public class MediaStorageManagementOptions
{
    // Get or set the maximum storage qoute for a tenant in bytes. Default is 1GB.
    public long MaximumStorageQuote { get; set; } = 1_073_741_824;
}
