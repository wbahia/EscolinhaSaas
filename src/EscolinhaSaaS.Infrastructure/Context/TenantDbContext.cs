using Microsoft.EntityFrameworkCore;
using EscolinhaSaaS.Domain.Entities;
using EscolinhaSaaS.Domain.Interfaces;
using System.Data;

namespace EscolinhaSaaS.Infrastructure.Context;

public class TenantDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public TenantDbContext(DbContextOptions<TenantDbContext> options, ITenantService tenantService) 
        : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Plano> Planos { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Aluno> Alunos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity => {
            entity.ToTable("tenants", "public"); 
            entity.HasIndex(e => e.Subdomain).IsUnique();
        });

        modelBuilder.Entity<User>(entity => {
            entity.ToTable("users", "public");
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Plano>(entity => {
            entity.ToTable("planos", "public");
            entity.HasData(
                new Plano { Id = 1, Nome = "Básico", Preco = 0 },
                new Plano { Id = 2, Nome = "Profissional", Preco = 99.90m },
                new Plano { Id = 3, Nome = "Premium", Preco = 199.90m }
            );
        });

        modelBuilder.Entity<Aluno>(entity => {
            entity.ToTable("alunos"); 
            entity.HasKey(e => e.Id);
            entity.Metadata.SetIsTableExcludedFromMigrations(true);
            entity.Property(e => e.DataNascimento).HasColumnName("data_nascimento");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
        });
    }

}