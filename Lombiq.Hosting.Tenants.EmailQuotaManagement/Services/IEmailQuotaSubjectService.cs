using Microsoft.Extensions.Localization;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

public interface IEmailQuotaSubjectService
{
    public LocalizedString GetWarningEmailSubject(int percentage);
    public LocalizedString GetExceededEmailSubject();
}
