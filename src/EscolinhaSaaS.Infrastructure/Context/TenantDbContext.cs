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
            entity.ToTable("Tenants", "public"); // Esquema fixo
            entity.HasIndex(e => e.Subdomain).IsUnique();
            entity.Property(e => e.Subdomain).IsRequired();
        });

        modelBuilder.Entity<User>(entity => {
            entity.ToTable("Users", "public"); // Esquema fixo
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Plano>(entity => {
            entity.ToTable("Planos", "public"); // Esquema fixo
            entity.HasData(
                new Plano { Id = 1, Nome = "Básico", Preco = 0 },
                new Plano { Id = 2, Nome = "Profissional", Preco = 99.90m },
                new Plano { Id = 3, Nome = "Premium", Preco = 199.90m }
            );
        });

        modelBuilder.Entity<Aluno>(entity => {
            entity.ToTable("alunos"); 
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nome).HasColumnName("nome");
            entity.Property(e => e.Matricula).HasColumnName("matricula");
            entity.Property(e => e.DataNascimento).HasColumnName("datanascimento");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.Property(e => e.TenantId).HasColumnName("tenantid");
            
            entity.Metadata.SetIsTableExcludedFromMigrations(true);
        });

        base.OnModelCreating(modelBuilder);
    }

}