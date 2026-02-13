namespace EscolinhaSaaS.Domain.Entities;

public class Aluno
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string Matricula { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public Guid TenantId { get; set; } 
}