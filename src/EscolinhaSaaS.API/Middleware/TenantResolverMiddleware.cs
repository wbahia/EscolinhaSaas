using EscolinhaSaaS.Domain.Interfaces;
using EscolinhaSaaS.Infrastructure.Context;

namespace EscolinhaSaaS.API.Middleware;

public class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService, TenantDbContext dbContext)
    {
        // 1. Extrai o TenantId do Token (sua lógica atual)
        var tenantIdClaim = context.User.FindFirst("tenantId")?.Value;

        if (!string.IsNullOrEmpty(tenantIdClaim))
        {
            tenantService.TenantId = Guid.Parse(tenantIdClaim);
        }

        await _next(context);
    }
}