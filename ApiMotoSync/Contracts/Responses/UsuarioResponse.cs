namespace ApiMotoSync.Contracts.Responses;

public sealed record UsuarioResponse(
    Guid Id,
    string Nome,
    string Email,
    string Cargo,
    Guid FilialId,
    string FilialNome,
    int MotosGerenciadas);

