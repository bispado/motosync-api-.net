namespace ApiMotoSync.Domain.Entities;

public class Filial
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Codigo { get; set; } = null!;
    public string Endereco { get; set; } = null!;
    public string Cidade { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<Moto> Motos { get; set; } = new List<Moto>();
}

