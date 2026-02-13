using System.Text;
using EscolinhaSaaS.Infrastructure.Context;
using EscolinhaSaaS.Domain.Interfaces;
using EscolinhaSaaS.Infrastructure.Services; 
using EscolinhaSaaS.Application.Services;
using EscolinhaSaaS.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// 1. Configuração do DbContext Principal
builder.Services.AddScoped<TenantSchemaInterceptor>();
builder.Services.AddDbContext<TenantDbContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<TenantSchemaInterceptor>();
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention()
        .AddInterceptors(interceptor); 
});

// 2. Configuração do DbContext do Schema do Tenant (usado apenas para gerar SQL)
// Não precisa do interceptor pois é usado apenas em design-time
builder.Services.AddDbContext<TenantSchemaDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention();
});

// 3. Registro dos serviços
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<TenantDatabaseService>();

// 4. Configuração do JWT
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"]!);
builder.Services.AddAuthentication(x => {
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x => {
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"]
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Escolinha SaaS API", Version = "v1" });

    // Define o esquema de segurança (Bearer Token)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Aplica a segurança globalmente no Swagger
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();

// O TenantResolver deve vir após a autenticação para ler o Claim do Token
app.UseMiddleware<TenantResolverMiddleware>(); 
app.UseAuthorization();

app.MapControllers();
app.Run();