using Microsoft.EntityFrameworkCore;
using EscolinhaSaaS.Domain.Entities;
using EscolinhaSaaS.Domain.Interfaces;

namespace EscolinhaSaaS.Infrastructure.Context;

public class TenantDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public TenantDbContext(DbContextOptions<TenantDbContext> options, ITenantService tenantService) 
        : base(options)
    {
        _tenantService = tenantService;
    }

    // Tabelas do Schema Public
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Plano> Planos { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurações do Schema Public
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<Tenant>(entity => {
            entity.HasIndex(e => e.Subdomain).IsUnique();
            entity.Property(e => e.Subdomain).IsRequired();
        });

        modelBuilder.Entity<User>(entity => {
            entity.HasIndex(e => e.Email).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }

    // Método crucial: Altera o schema da conexão antes de qualquer query
    public async Task UpdateSchemaAsync()
    {
        if (_tenantService.TenantId.HasValue)
        {
            var schemaName = $"escolinha_{_tenantService.TenantId.Value.ToString().Replace("-", "")}";
            // Comando PostgreSQL para setar o search_path
            await Database.ExecuteSqlRawAsync($"SET search_path TO {schemaName}, public;");
        }
    }
}