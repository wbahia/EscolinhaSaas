using EscolinhaSaaS.Domain.Interfaces;
using System.Security.Claims;

namespace EscolinhaSaaS.API.Middleware;

public class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        // Se o usuário estiver autenticado, extraímos o tenantId das Claims do JWT
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("tenantId")?.Value;
            var subdomainClaim = context.User.FindFirst("subdomain")?.Value;

            if (Guid.TryParse(tenantIdClaim, out Guid tenantId))
            {
                tenantService.SetTenant(tenantId, subdomainClaim ?? string.Empty);
            }
        }

        await _next(context);
    }
}