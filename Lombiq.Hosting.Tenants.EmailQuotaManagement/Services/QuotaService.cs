﻿using Lombiq.Hosting.Tenants.EmailQuotaManagement.Indexes;
using Lombiq.Hosting.Tenants.EmailQuotaManagement.Models;
using System.Threading.Tasks;
using YesSql;

namespace Lombiq.Hosting.Tenants.EmailQuotaManagement.Services;

public class QuotaService : IQuotaService
{
    private readonly ISession _session;

    public QuotaService(ISession session)
    {
        _session = session;
    }

    public async Task<EmailQuota> GetCurrentQuotaAsync()
    {
        var currentQuota = await _session.Query<EmailQuota, EmailQuotaIndex>().FirstOrDefaultAsync();

        if (currentQuota != null) return currentQuota;

        currentQuota = new EmailQuota();
        _session.Save(currentQuota);

        return currentQuota;
    }

    public void IncreaseQuota(EmailQuota emailQuota)
    {
        emailQuota.CurrentEmailQuotaCount++;
        _session.Save(emailQuota);
    }

    public void ResetQuota(EmailQuota emailQuota)
    {
        emailQuota.CurrentEmailQuotaCount = 0;
        _session.Save(emailQuota);
    }
}