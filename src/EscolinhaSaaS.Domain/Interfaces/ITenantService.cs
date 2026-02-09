namespace EscolinhaSaaS.Domain.Interfaces;

public interface ITenantService
{
    Guid? TenantId { get; }
    string? Subdomain { get; }
    void SetTenant(Guid tenantId, string subdomain);
}