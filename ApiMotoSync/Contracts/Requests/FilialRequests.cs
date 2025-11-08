using System.ComponentModel.DataAnnotations;

namespace ApiMotoSync.Contracts.Requests;

public sealed record CreateFilialRequest(
    [Required, StringLength(160)] string Nome,
    [Required, StringLength(20)] string Codigo,
    [Required, StringLength(200)] string Endereco,
    [Required, StringLength(120)] string Cidade,
    [Required, StringLength(60)] string Estado);

public sealed record UpdateFilialRequest(
    [Required, StringLength(160)] string Nome,
    [Required, StringLength(200)] string Endereco,
    [Required, StringLength(120)] string Cidade,
    [Required, StringLength(60)] string Estado);

