using Microsoft.EntityFrameworkCore;
using EscolinhaSaaS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace EscolinhaSaaS.Infrastructure.Services;

public class TenantDatabaseService
{
    private readonly TenantDbContext _dbContext;

    public TenantDatabaseService(TenantDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateTenantStructureAsync(Guid tenantId)
    {
        var schemaName = $"escolinha_{tenantId:n}";
        
        // SQL direto garante que não criaremos tabelas do schema 'public' repetidas
        var sql = $@"
            SET search_path TO ""{schemaName}"";

            CREATE TABLE IF NOT EXISTS alunos (
                id uuid NOT NULL,
                nome text NOT NULL,
                data_nascimento timestamp with time zone NOT NULL,
                email text,
                tenant_id uuid NOT NULL,
                CONSTRAINT pk_alunos PRIMARY KEY (id)
            );

            CREATE INDEX IF NOT EXISTS ix_alunos_tenant_id ON alunos (tenant_id);
        ";

        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        
        // Aproveita a transação existente no AuthService
        if (_dbContext.Database.CurrentTransaction != null)
            command.Transaction = _dbContext.Database.CurrentTransaction.GetDbTransaction();

        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }
}