using ApiMotoSync.Contracts.Requests;
using ApiMotoSync.Contracts.Responses;
using ApiMotoSync.Domain.Entities;
using ApiMotoSync.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiMotoSync.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FiliaisController : ControllerBase
{
    private readonly MotoSyncContext _context;

    public FiliaisController(MotoSyncContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FilialResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FilialResponse>>> GetFiliais(CancellationToken cancellationToken)
    {
        var filiais = await _context.Filiais
            .AsNoTracking()
            .Select(filial => new FilialResponse(
                filial.Id,
                filial.Nome,
                filial.Codigo,
                filial.Endereco,
                filial.Cidade,
                filial.Estado,
                filial.Usuarios.Count,
                filial.Motos.Count))
            .ToListAsync(cancellationToken);

        return Ok(filiais);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FilialResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FilialResponse>> GetFilial(Guid id, CancellationToken cancellationToken)
    {
        var filial = await _context.Filiais
            .AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new FilialResponse(
                f.Id,
                f.Nome,
                f.Codigo,
                f.Endereco,
                f.Cidade,
                f.Estado,
                f.Usuarios.Count,
                f.Motos.Count))
            .FirstOrDefaultAsync(cancellationToken);

        if (filial is null)
        {
            return NotFound();
        }

        return Ok(filial);
    }

    [HttpPost]
    [ProducesResponseType(typeof(FilialResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FilialResponse>> CreateFilial(
        [FromBody] CreateFilialRequest request,
        CancellationToken cancellationToken)
    {
        var codigoEmUso = await _context.Filiais
            .AnyAsync(f => f.Codigo == request.Codigo, cancellationToken);

        if (codigoEmUso)
        {
            return Conflict($"Código {request.Codigo} já utilizado por outra filial.");
        }

        var filial = new Filial
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Codigo = request.Codigo,
            Endereco = request.Endereco,
            Cidade = request.Cidade,
            Estado = request.Estado
        };

        await _context.Filiais.AddAsync(filial, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new FilialResponse(
            filial.Id,
            filial.Nome,
            filial.Codigo,
            filial.Endereco,
            filial.Cidade,
            filial.Estado,
            0,
            0);

        return CreatedAtAction(nameof(GetFilial), new { id = filial.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFilial(
        Guid id,
        [FromBody] UpdateFilialRequest request,
        CancellationToken cancellationToken)
    {
        var filial = await _context.Filiais
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

        if (filial is null)
        {
            return NotFound();
        }

        filial.Nome = request.Nome;
        filial.Endereco = request.Endereco;
        filial.Cidade = request.Cidade;
        filial.Estado = request.Estado;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteFilial(Guid id, CancellationToken cancellationToken)
    {
        var filial = await _context.Filiais
            .Include(f => f.Usuarios)
            .Include(f => f.Motos)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

        if (filial is null)
        {
            return NotFound();
        }

        if (filial.Usuarios.Any() || filial.Motos.Any())
        {
            return Conflict("Remova usuários e motos associados antes de excluir a filial.");
        }

        _context.Filiais.Remove(filial);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

