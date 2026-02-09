using System;

namespace EscolinhaSaaS.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegação para o Tenant (Schema Public)
    public virtual Tenant Tenant { get; set; } = null!;
}

public enum UserRole
{
    Admin,
    Gestor,
    Professor
}