namespace ApiMotoSync.Domain.Entities;

public class Usuario
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Cargo { get; set; } = null!;
    public Guid FilialId { get; set; }
    public Filial? Filial { get; set; }
    public ICollection<Moto> MotosGerenciadas { get; set; } = new List<Moto>();
}

