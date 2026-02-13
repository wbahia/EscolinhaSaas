using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using EscolinhaSaaS.Domain.Interfaces;


namespace EscolinhaSaaS.Infrastructure.Context;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../EscolinhaSaaS.API"))
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

        // 2. Mock do ITenantService (já que em design time não há request/tenant ativo)
        var tenantServiceMock = new DesignTimeTenantService();

        return new TenantDbContext(optionsBuilder.Options, tenantServiceMock);
    }
}

// Classe auxiliar simples apenas para satisfazer o construtor do DbContext no Design Time
public class DesignTimeTenantService : ITenantService
{
    public Guid? TenantId { get; set; } = null;
    public string? Subdomain { get; set; } = null;

    public void SetTenant(Guid tenantId, string subdomain) 
    { 
        TenantId = tenantId;
        Subdomain = subdomain;
    }
}