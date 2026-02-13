using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EscolinhaSaaS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlunosTemplateToFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION create_tenant_schema(tenant_id TEXT)
                RETURNS void AS $$
                DECLARE
                    schema_name TEXT := 'escolinha_' || replace(tenant_id, '-', '');
                BEGIN
                    EXECUTE 'CREATE SCHEMA IF NOT EXISTS ' || schema_name;

                    -- Criar tabela de Alunos dentro do novo schema
                    EXECUTE 'CREATE TABLE IF NOT EXISTS ' || schema_name || '.Alunos (
                        Id UUID PRIMARY KEY,
                        Nome VARCHAR(200) NOT NULL,
                        DataNascimento TIMESTAMP NOT NULL,
                        Matricula VARCHAR(50) NOT NULL,
                        Ativo BOOLEAN DEFAULT TRUE,
                        TenantId UUID NOT NULL
                    )';
                    
                    -- Você pode adicionar índices aqui também
                    EXECUTE 'CREATE INDEX IF NOT EXISTS idx_alunos_tenant ON ' || schema_name || '.Alunos(TenantId)';
                END;
                $$ LANGUAGE plpgsql;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
