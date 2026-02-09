using EscolinhaSaaS.Domain.Interfaces;
using EscolinhaSaaS.Infrastructure.Context;
using EscolinhaSaaS.Infrastructure.Services;
using EscolinhaSaaS.API.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar DB Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TenantDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Registrar Injeção de Dependência
builder.Services.AddScoped<ITenantService, TenantService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Middlewares (A ORDEM IMPORTA!)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// O TenantResolver deve vir DEPOIS da Autenticação (quando implementarmos)
// Mas para teste inicial de Dashboard, ele fica aqui:
app.UseMiddleware<TenantResolverMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();