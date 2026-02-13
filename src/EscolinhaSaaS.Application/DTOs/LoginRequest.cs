namespace EscolinhaSaaS.Application.DTOs;

public record LoginRequest(string Email, string Senha, string Subdomain);
