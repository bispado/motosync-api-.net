namespace ApiMotoSync.Domain.Entities;

public class Moto
{
    public Guid Id { get; set; }
    public string Modelo { get; set; } = null!;
    public string Placa { get; set; } = null!;
    public int Ano { get; set; }
    public string Status { get; set; } = "Disponível";
    public Guid FilialId { get; set; }
    public Filial? Filial { get; set; }
    public Guid? GestorId { get; set; }
    public Usuario? Gestor { get; set; }
}

