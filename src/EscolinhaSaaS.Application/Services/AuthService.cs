using EscolinhaSaaS.Application.DTOs;
using EscolinhaSaaS.Domain.Entities;
using EscolinhaSaaS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace EscolinhaSaaS.Application.Services;

public class AuthService
{
    private readonly TenantDbContext _context;
    private readonly TokenService _tokenService;

    public AuthService(TenantDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<Guid> RegisterEscolinhaAsync(RegisterRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var tenant = new Tenant
            {
                Nome = request.NomeEscolinha,
                Subdomain = request.Subdomain.ToLower(),
                Cnpj = request.Cnpj,
                PlanoId = 1
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            var user = new User
            {
                Nome = request.Nome,
                Email = request.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
                Role = UserRole.Admin,
                TenantId = tenant.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Chama a função SQL de criação de schema do PostgreSQL
            await _context.Database.ExecuteSqlRawAsync("SELECT public.create_tenant_schema({0})", tenant.Id);

            await transaction.CommitAsync();
            return tenant.Id;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<string> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Tenant.Subdomain == request.Subdomain);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Senha, user.SenhaHash))
            throw new Exception("Credenciais inválidas.");

        return _tokenService.GenerateToken(user);
    }
}