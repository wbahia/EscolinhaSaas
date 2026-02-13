using Microsoft.EntityFrameworkCore;
using EscolinhaSaaS.Domain.Entities;

namespace EscolinhaSaaS.Infrastructure.Context;

/// <summary>
/// DbContext usado APENAS para gerar a estrutura das tabelas do tenant.
/// Não é usado em runtime - apenas para geração de SQL.
/// </summary>
public class TenantSchemaDbContext : DbContext
{
    public DbSet<Aluno> Alunos { get; set; }
    
    // TODO: Adicione aqui outras entidades do tenant
    // public DbSet<Turma> Turmas { get; set; }
    // public DbSet<Professor> Professores { get; set; }

    public TenantSchemaDbContext(DbContextOptions<TenantSchemaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuração da entidade Aluno
        modelBuilder.Entity<Aluno>(entity =>
        {
            entity.ToTable("alunos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired();
            entity.Property(e => e.DataNascimento).IsRequired();
            entity.Property(e => e.Matricula).IsRequired();
            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.TenantId).IsRequired();
            entity.HasIndex(e => e.TenantId);
        });

        // TODO: Configure aqui outras entidades do tenant
        // modelBuilder.Entity<Turma>(entity =>
        // {
        //     entity.ToTable("turmas");
        //     entity.HasKey(e => e.Id);
        //     // ... outras configurações
        // });
    }
}