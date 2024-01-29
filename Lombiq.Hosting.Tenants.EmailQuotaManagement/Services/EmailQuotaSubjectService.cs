using Microsoft.Extensions.Localization;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

public class EmailQuotaSubjectService(IStringLocalizer<EmailQuotaSubjectService> stringLocalizer) : IEmailQuotaSubjectService
{
    private readonly IStringLocalizer T = stringLocalizer;

    public LocalizedString GetWarningEmailSubject(int percentage) =>
        T["[Warning] Your site has used {0}% of its e-mail quota", percentage];

    public LocalizedString GetExceededEmailSubject() =>
        T["[Action Required] Your site has run over its e-mail quota"];
}
