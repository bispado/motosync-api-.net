namespace ApiMotoSync.Contracts.Responses;

public sealed record MotoResponse(
    Guid Id,
    string Modelo,
    string Placa,
    int Ano,
    string Status,
    Guid FilialId,
    string FilialNome,
    Guid? GestorId,
    string? GestorNome);

