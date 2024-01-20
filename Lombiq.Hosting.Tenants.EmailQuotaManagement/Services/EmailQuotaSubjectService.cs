using Microsoft.Extensions.Localization;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

public class EmailQuotaSubjectService(IStringLocalizer<EmailQuotaSubjectService> stringLocalizer) : IEmailQuotaSubjectService
{
    public LocalizedString GetWarningEmailSubject(int percentage) =>
        stringLocalizer["[Warning] Your site has used {0}% of its e-mail quota", percentage];

    public LocalizedString GetExceededEmailSubject() =>
        stringLocalizer["[Action Required] Your site has run over its e-mail quota"];
}
