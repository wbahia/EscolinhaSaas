using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using EscolinhaSaaS.Infrastructure.Context;
using Npgsql;
using Microsoft.EntityFrameworkCore.Storage;

namespace EscolinhaSaaS.Infrastructure.Services;

public class TenantDatabaseService
{
    private readonly TenantDbContext _dbContext;
    private readonly string _connectionString;

    public TenantDatabaseService(TenantDbContext dbContext)
    {
        _dbContext = dbContext;
        _connectionString = dbContext.Database.GetConnectionString()!;
    }

    public async Task CreateTenantStructureAsync(Guid tenantId)
    {
        var schemaName = $"escolinha_{tenantId:N}";
        
        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        using var command = connection.CreateCommand();
        
        // Aproveita a transação existente
        if (_dbContext.Database.CurrentTransaction != null)
            command.Transaction = _dbContext.Database.CurrentTransaction.GetDbTransaction();

        // 1. Criar o schema
        command.CommandText = $@"CREATE SCHEMA IF NOT EXISTS ""{schemaName}"";";
        await command.ExecuteNonQueryAsync();

        // 2. Gerar e executar o SQL das tabelas do tenant
        var sql = GenerateTenantTablesSql(schemaName);
        
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private string GenerateTenantTablesSql(string schemaName)
    {
        // Cria um DbContext temporário só para gerar o modelo
        var optionsBuilder = new DbContextOptionsBuilder<TenantSchemaDbContext>();
        optionsBuilder.UseNpgsql(_connectionString)
                    .UseSnakeCaseNamingConvention();

        using var tempContext = new TenantSchemaDbContext(optionsBuilder.Options);
        
        var model = tempContext.Model;
        var sqlGenerator = tempContext.GetService<IMigrationsSqlGenerator>();
        
        var operations = new List<MigrationOperation>();

        // Gera as operações para cada entidade
        foreach (var entityType in model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName()!;
            
            var createTable = new CreateTableOperation
            {
                Schema = schemaName,
                Name = tableName
            };

            // Adiciona as colunas
            foreach (var property in entityType.GetProperties())
            {
                createTable.Columns.Add(new AddColumnOperation
                {
                    Schema = schemaName,
                    Table = tableName,
                    Name = property.GetColumnName(),
                    ClrType = property.ClrType,
                    ColumnType = property.GetColumnType(),
                    IsNullable = property.IsNullable,
                    DefaultValue = property.GetDefaultValue(),
                    DefaultValueSql = property.GetDefaultValueSql(),
                    ComputedColumnSql = property.GetComputedColumnSql()
                });
            }

            // Adiciona a chave primária
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey != null)
            {
                createTable.PrimaryKey = new AddPrimaryKeyOperation
                {
                    Schema = schemaName,
                    Table = tableName,
                    Name = primaryKey.GetName() ?? $"pk_{tableName}",
                    Columns = primaryKey.Properties.Select(p => p.GetColumnName() ?? p.Name).ToArray()
                };
            }

            operations.Add(createTable);

            // Adiciona os índices
            foreach (var index in entityType.GetIndexes())
            {
                operations.Add(new CreateIndexOperation
                {
                    Schema = schemaName,
                    Table = tableName,
                    Name = index.GetDatabaseName() ?? $"ix_{tableName}_{string.Join("_", index.Properties.Select(p => p.Name))}",
                    Columns = index.Properties.Select(p => p.GetColumnName() ?? p.Name).ToArray(),
                    IsUnique = index.IsUnique,
                    Filter = index.GetFilter()
                });
            }

            // Adiciona foreign keys se houver
            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                var fkName = foreignKey.GetConstraintName() ?? 
                            $"fk_{tableName}_{foreignKey.PrincipalEntityType.GetTableName()}";
                
                operations.Add(new AddForeignKeyOperation
                {
                    Schema = schemaName,
                    Table = tableName,
                    Name = fkName,
                    Columns = foreignKey.Properties.Select(p => p.GetColumnName() ?? p.Name).ToArray(),
                    PrincipalSchema = foreignKey.PrincipalEntityType.GetSchema() ?? schemaName,
                    PrincipalTable = foreignKey.PrincipalEntityType.GetTableName()!,
                    PrincipalColumns = foreignKey.PrincipalKey.Properties.Select(p => p.GetColumnName() ?? p.Name).ToArray(),
                    OnDelete = foreignKey.DeleteBehavior switch
                    {
                        DeleteBehavior.Cascade => ReferentialAction.Cascade,
                        DeleteBehavior.SetNull => ReferentialAction.SetNull,
                        DeleteBehavior.Restrict => ReferentialAction.Restrict,
                        _ => ReferentialAction.NoAction
                    }
                });
            }
        }

        // Gera o SQL a partir das operações
        var sqlCommands = sqlGenerator.Generate(operations, model);
        
        return string.Join("\n", sqlCommands.Select(c => c.CommandText));
    }
}