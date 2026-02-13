namespace EscolinhaSaaS.Application.DTOs;

public record RegisterRequest(
    string Nome, 
    string Email, 
    string Senha, 
    string NomeEscolinha, 
    string Subdomain, 
    string Cnpj);