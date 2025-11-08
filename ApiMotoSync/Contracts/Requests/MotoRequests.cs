using System.ComponentModel.DataAnnotations;

namespace ApiMotoSync.Contracts.Requests;

public sealed record CreateMotoRequest(
    [Required, StringLength(150)] string Modelo,
    [Required, StringLength(10)] string Placa,
    [Range(1980, 2100)] int Ano,
    Guid FilialId,
    Guid? GestorId,
    [StringLength(50)] string? Status);

public sealed record UpdateMotoRequest(
    [Required, StringLength(150)] string Modelo,
    [Range(1980, 2100)] int Ano,
    Guid FilialId,
    Guid? GestorId,
    [StringLength(50)] string? Status);

