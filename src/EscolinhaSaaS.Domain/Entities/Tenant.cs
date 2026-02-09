using System;

namespace EscolinhaSaaS.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public string? LogoUrl { get; set; }
    public string CorPrimaria { get; set; } = "#1E40AF";
    public string CorSecundaria { get; set; } = "#3B82F6";
    public int PlanoId { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Propriedade de navegação
    public virtual Plano Plano { get; set; } = null!;
}