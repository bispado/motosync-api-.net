using ApiMotoSync.Contracts.Requests;
using ApiMotoSync.Contracts.Responses;
using ApiMotoSync.Domain.Entities;
using ApiMotoSync.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiMotoSync.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly MotoSyncContext _context;

    public UsuariosController(MotoSyncContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UsuarioResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UsuarioResponse>>> GetUsuarios(CancellationToken cancellationToken)
    {
        var usuarios = await _context.Usuarios
            .AsNoTracking()
            .Select(usuario => new UsuarioResponse(
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.Cargo,
                usuario.FilialId,
                usuario.Filial != null ? usuario.Filial.Nome : string.Empty,
                usuario.MotosGerenciadas.Count))
            .ToListAsync(cancellationToken);

        return Ok(usuarios);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponse>> GetUsuario(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .Where(usuario => usuario.Id == id)
            .Select(usuario => new UsuarioResponse(
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.Cargo,
                usuario.FilialId,
                usuario.Filial != null ? usuario.Filial.Nome : string.Empty,
                usuario.MotosGerenciadas.Count))
            .FirstOrDefaultAsync(cancellationToken);

        if (usuario is null)
        {
            return NotFound();
        }

        return Ok(usuario);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioResponse>> CreateUsuario(
        [FromBody] CreateUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var filialExists = await _context.Filiais
            .AnyAsync(filial => filial.Id == request.FilialId, cancellationToken);

        if (!filialExists)
        {
            return NotFound($"Filial {request.FilialId} não encontrada.");
        }

        var emailUso = await _context.Usuarios
            .AnyAsync(usuario => usuario.Email == request.Email, cancellationToken);

        if (emailUso)
        {
            return Conflict($"E-mail {request.Email} já cadastrado.");
        }

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Email = request.Email,
            Cargo = request.Cargo,
            FilialId = request.FilialId
        };

        await _context.Usuarios.AddAsync(usuario, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.Id == usuario.Id)
            .Select(u => new UsuarioResponse(
                u.Id,
                u.Nome,
                u.Email,
                u.Cargo,
                u.FilialId,
                u.Filial != null ? u.Filial.Nome : string.Empty,
                u.MotosGerenciadas.Count))
            .FirstAsync(cancellationToken);

        return CreatedAtAction(nameof(GetUsuario), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUsuario(
        Guid id,
        [FromBody] UpdateUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(usuario => usuario.Id == id, cancellationToken);

        if (usuario is null)
        {
            return NotFound();
        }

        usuario.Nome = request.Nome;
        usuario.Cargo = request.Cargo;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUsuario(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .Include(usuario => usuario.MotosGerenciadas)
            .FirstOrDefaultAsync(usuario => usuario.Id == id, cancellationToken);

        if (usuario is null)
        {
            return NotFound();
        }

        foreach (var moto in usuario.MotosGerenciadas)
        {
            moto.GestorId = null;
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

