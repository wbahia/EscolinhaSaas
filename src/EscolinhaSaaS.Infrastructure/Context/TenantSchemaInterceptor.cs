using System.Data.Common;
using EscolinhaSaaS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EscolinhaSaaS.Infrastructure.Context;

public class TenantSchemaInterceptor : DbConnectionInterceptor
{
    private readonly ITenantService _tenantService;

    public TenantSchemaInterceptor(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (_tenantService.TenantId.HasValue)
        {
            var schemaName = $"escolinha_{_tenantService.TenantId.Value:n}";
            
            using var command = connection.CreateCommand();
            // Adicionei o comando para garantir que não haja confusão com Case Sensitivity
            command.CommandText = $"SET search_path TO \"{schemaName}\", public;";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }
}