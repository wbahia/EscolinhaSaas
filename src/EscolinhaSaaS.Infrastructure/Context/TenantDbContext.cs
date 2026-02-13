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

    // Tabelas do schema public
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Plano> Planos { get; set; }
    public DbSet<User> Users { get; set; }
    
    // DbSet para uso em runtime (tabelas nos schemas dos tenants)
    public DbSet<Aluno> Alunos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ========================================
        // TABELAS DO SCHEMA PUBLIC
        // ========================================
        
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
                new Plano { Id = 1, Nome = "Básico", Preco = 0, FeaturesJson = "{}", MaxAlunos = 0, Ativo = true },
                new Plano { Id = 2, Nome = "Profissional", Preco = 99.90m, FeaturesJson = "{}", MaxAlunos = 0, Ativo = true },
                new Plano { Id = 3, Nome = "Premium", Preco = 199.90m, FeaturesJson = "{}", MaxAlunos = 0, Ativo = true }
            );
        });

        // ========================================
        // TABELAS DOS SCHEMAS DOS TENANTS
        // ========================================
        
        // Configuração de Aluno - funciona em runtime mas não gera migrations
        modelBuilder.Entity<Aluno>(entity => {
            entity.ToTable("alunos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired();
            entity.Property(e => e.DataNascimento).IsRequired();
            entity.Property(e => e.Matricula).IsRequired();
            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.TenantId).IsRequired();
            entity.HasIndex(e => e.TenantId);
            
            // CRÍTICO: Exclui da migration mas mantém no modelo do EF Core
            entity.Metadata.SetIsTableExcludedFromMigrations(true);
        });

        // TODO: Adicione aqui outras entidades do tenant com a mesma configuração
        // modelBuilder.Entity<Turma>(entity => {
        //     entity.ToTable("turmas");
        //     // ... configurações
        //     entity.Metadata.SetIsTableExcludedFromMigrations(true);
        // });
    }

    public override int SaveChanges()
    {
        SetTenantId();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTenantId();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetTenantId()
    {
        // Só aplica se houver um tenant ativo no contexto da requisição
        if (!_tenantService.TenantId.HasValue)
            return;

        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            // Verifica se a entidade tem a propriedade TenantId
            var tenantIdProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "TenantId");
            
            if (tenantIdProperty != null)
            {
                var currentValue = (Guid?)tenantIdProperty.CurrentValue;
                
                // Se está adicionando E o TenantId está vazio/zerado, preenche automaticamente
                if (entry.State == EntityState.Added && (currentValue == null || currentValue == Guid.Empty))
                {
                    tenantIdProperty.CurrentValue = _tenantService.TenantId.Value;
                }
                
                // Se está modificando, garante que o TenantId não foi alterado
                if (entry.State == EntityState.Modified)
                {
                    // Impede alteração do TenantId em updates
                    tenantIdProperty.IsModified = false;
                }
            }
        }
    }
}