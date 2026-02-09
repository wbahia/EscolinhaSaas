namespace EscolinhaSaaS.Domain.Entities;

public class Plano
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int MaxAlunos { get; set; }
    public string FeaturesJson { get; set; } = "{}"; // Armazenado como string/jsonb
    public bool Ativo { get; set; } = true;
}