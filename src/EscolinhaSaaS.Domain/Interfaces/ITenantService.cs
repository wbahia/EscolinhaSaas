namespace EscolinhaSaaS.Domain.Interfaces;

public interface ITenantService
{
    Guid? TenantId { get; set; }
    string? Subdomain { get; }
    void SetTenant(Guid tenantId, string subdomain);
}