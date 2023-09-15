using Lombiq.HelpfulExtensions.Extensions.Emails.Services;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using System.Threading.Tasks;
using Lombiq.HelpfulExtensions.Extensions.Emails.Extensions;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Email;
using OrchardCore.Email.Services;
using OrchardCore.Security;
using OrchardCore.Security.Services;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Linq;
using static OrchardCore.Security.Permissions.Permission;
using static OrchardCore.Security.StandardPermissions;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

public class EmailQuotaEmailService : IEmailQuotaEmailService
{
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly ShellSettings _shellSettings;
    private readonly IRoleService _roleService;
    private readonly UserManager<IUser> _userManager;

    public EmailQuotaEmailService(
        IEmailTemplateService emailTemplateService,
        ShellSettings shellSettings,
        IRoleService roleService,
        UserManager<IUser> userManager)
    {
        _emailTemplateService = emailTemplateService;
        _shellSettings = shellSettings;
        _roleService = roleService;
        _userManager = userManager;
    }

    public async Task<MailMessage> CreateEmailForExceedingQuota()
    {
        var emailTemplate = await _emailTemplateService.RenderEmailTemplateAsync("EmailQuote", new
        {
            HostName = _shellSettings.Name,
        });

        // Get users with site owner permission.
        var roles = await _roleService.GetRolesAsync();
        var siteOwnerRoles = roles.Where(role =>
            (role as Role)?.RoleClaims.Exists(claim =>
                claim.ClaimType == ClaimType && claim.ClaimValue == SiteOwner.Name) == true);

        var siteOwners = new List<IUser>();
        foreach (var role in siteOwnerRoles)
        {
            siteOwners.AddRange(await _userManager.GetUsersInRoleAsync(role.RoleName));
        }

        var siteOwnerEmails = siteOwners.Select(user => (user as User)?.Email);
        var emailMessage = new MailMessage
        {
            Bcc = siteOwnerEmails.Join(","),
            Subject = "[Action Required] Your DotNest site has run over its e-mail quota",
            Body = emailTemplate,
            IsHtmlBody = true,
        };

        return emailMessage;
    }
}
