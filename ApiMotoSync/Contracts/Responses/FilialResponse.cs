namespace ApiMotoSync.Contracts.Responses;

public sealed record FilialResponse(
    Guid Id,
    string Nome,
    string Codigo,
    string Endereco,
    string Cidade,
    string Estado,
    int UsuariosRegistrados,
    int MotosDisponiveis);

