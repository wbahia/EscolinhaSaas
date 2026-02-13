using EscolinhaSaaS.Domain.Interfaces;

namespace EscolinhaSaaS.Infrastructure.Services;

public class TenantService : ITenantService
{
    public Guid? TenantId { get; set; }
    public string? Subdomain { get; private set; }

    public void SetTenant(Guid tenantId, string subdomain)
    {
        TenantId = tenantId;
        Subdomain = subdomain;
    }
}