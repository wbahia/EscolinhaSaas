-- Script de inicialização do banco de dados
-- Executado automaticamente quando o container PostgreSQL é criado

-- Habilitar extensão UUID
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Criar schema public (já existe por padrão, mas garantimos)
CREATE SCHEMA IF NOT EXISTS public;

-- Comentários para documentação
COMMENT ON SCHEMA public IS 'Schema compartilhado entre todos os tenants - contém dados de tenants, users e planos';

-- Função para criar schema de tenant automaticamente
CREATE OR REPLACE FUNCTION create_tenant_schema(tenant_id UUID)
RETURNS void AS $$
DECLARE
    schema_name TEXT;
BEGIN
    schema_name := 'escolinha_' || REPLACE(tenant_id::TEXT, '-', '');
    
    -- Criar schema
    EXECUTE format('CREATE SCHEMA IF NOT EXISTS %I', schema_name);
    
    -- Adicionar comentário
    EXECUTE format('COMMENT ON SCHEMA %I IS ''Schema isolado para tenant ID: %s''', 
                   schema_name, tenant_id);
    
    RAISE NOTICE 'Schema % criado com sucesso', schema_name;
END;
$$ LANGUAGE plpgsql;

-- Função para deletar schema de tenant (usar com cuidado!)
CREATE OR REPLACE FUNCTION delete_tenant_schema(tenant_id UUID)
RETURNS void AS $$
DECLARE
    schema_name TEXT;
BEGIN
    schema_name := 'escolinha_' || REPLACE(tenant_id::TEXT, '-', '');
    
    -- Deletar schema e todo seu conteúdo
    EXECUTE format('DROP SCHEMA IF EXISTS %I CASCADE', schema_name);
    
    RAISE NOTICE 'Schema % deletado', schema_name;
END;
$$ LANGUAGE plpgsql;

-- Log de inicialização
DO $$
BEGIN
    RAISE NOTICE '===========================================';
    RAISE NOTICE 'Banco de dados escolinha_saas inicializado';
    RAISE NOTICE 'Extensão UUID habilitada';
    RAISE NOTICE 'Funções utilitárias criadas';
    RAISE NOTICE '===========================================';
END $$;
