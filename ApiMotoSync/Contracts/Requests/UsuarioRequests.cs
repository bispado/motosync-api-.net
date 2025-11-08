using System.ComponentModel.DataAnnotations;

namespace ApiMotoSync.Contracts.Requests;

public sealed record CreateUsuarioRequest(
    [Required, StringLength(150)] string Nome,
    [Required, EmailAddress, StringLength(180)] string Email,
    [Required, StringLength(80)] string Cargo,
    Guid FilialId);

public sealed record UpdateUsuarioRequest(
    [Required, StringLength(150)] string Nome,
    [Required, StringLength(80)] string Cargo);

