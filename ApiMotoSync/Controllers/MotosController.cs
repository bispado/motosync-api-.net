using ApiMotoSync.Contracts.Requests;
using ApiMotoSync.Contracts.Responses;
using ApiMotoSync.Domain.Entities;
using ApiMotoSync.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiMotoSync.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MotosController : ControllerBase
{
    private readonly MotoSyncContext _context;

    public MotosController(MotoSyncContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MotoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MotoResponse>>> GetMotos(CancellationToken cancellationToken)
    {
        var motos = await _context.Motos
            .AsNoTracking()
            .Select(moto => new MotoResponse(
                moto.Id,
                moto.Modelo,
                moto.Placa,
                moto.Ano,
                moto.Status,
                moto.FilialId,
                moto.Filial != null ? moto.Filial.Nome : string.Empty,
                moto.GestorId,
                moto.Gestor != null ? moto.Gestor.Nome : null))
            .ToListAsync(cancellationToken);

        return Ok(motos);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MotoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MotoResponse>> GetMoto(Guid id, CancellationToken cancellationToken)
    {
        var moto = await _context.Motos
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MotoResponse(
                m.Id,
                m.Modelo,
                m.Placa,
                m.Ano,
                m.Status,
                m.FilialId,
                m.Filial != null ? m.Filial.Nome : string.Empty,
                m.GestorId,
                m.Gestor != null ? m.Gestor.Nome : null))
            .FirstOrDefaultAsync(cancellationToken);

        if (moto is null)
        {
            return NotFound();
        }

        return Ok(moto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(MotoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MotoResponse>> CreateMoto(
        [FromBody] CreateMotoRequest request,
        CancellationToken cancellationToken)
    {
        var filial = await _context.Filiais
            .FirstOrDefaultAsync(f => f.Id == request.FilialId, cancellationToken);

        if (filial is null)
        {
            return NotFound($"Filial {request.FilialId} não encontrada.");
        }

        var placaEmUso = await _context.Motos
            .AnyAsync(m => m.Placa == request.Placa, cancellationToken);

        if (placaEmUso)
        {
            return Conflict($"Placa {request.Placa} já cadastrada.");
        }

        Usuario? gestor = null;
        if (request.GestorId.HasValue)
        {
            gestor = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.GestorId.Value, cancellationToken);

            if (gestor is null)
            {
                return NotFound($"Gestor {request.GestorId} não encontrado.");
            }

            if (gestor.FilialId != request.FilialId)
            {
                return Conflict("O gestor precisa estar associado à mesma filial da moto.");
            }
        }

        var moto = new Moto
        {
            Id = Guid.NewGuid(),
            Modelo = request.Modelo,
            Placa = request.Placa.ToUpperInvariant(),
            Ano = request.Ano,
            FilialId = request.FilialId,
            GestorId = request.GestorId,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Disponível" : request.Status!
        };

        await _context.Motos.AddAsync(moto, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = await _context.Motos
            .AsNoTracking()
            .Where(m => m.Id == moto.Id)
            .Select(m => new MotoResponse(
                m.Id,
                m.Modelo,
                m.Placa,
                m.Ano,
                m.Status,
                m.FilialId,
                m.Filial != null ? m.Filial.Nome : string.Empty,
                m.GestorId,
                m.Gestor != null ? m.Gestor.Nome : null))
            .FirstAsync(cancellationToken);

        return CreatedAtAction(nameof(GetMoto), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateMoto(
        Guid id,
        [FromBody] UpdateMotoRequest request,
        CancellationToken cancellationToken)
    {
        var moto = await _context.Motos
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (moto is null)
        {
            return NotFound();
        }

        var filial = await _context.Filiais
            .FirstOrDefaultAsync(f => f.Id == request.FilialId, cancellationToken);

        if (filial is null)
        {
            return NotFound($"Filial {request.FilialId} não encontrada.");
        }

        if (request.GestorId.HasValue)
        {
            var gestor = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.GestorId.Value, cancellationToken);

            if (gestor is null)
            {
                return NotFound($"Gestor {request.GestorId} não encontrado.");
            }

            if (gestor.FilialId != request.FilialId)
            {
                return Conflict("O gestor precisa estar associado à mesma filial da moto.");
            }
        }

        moto.Modelo = request.Modelo;
        moto.Ano = request.Ano;
        moto.FilialId = request.FilialId;
        moto.GestorId = request.GestorId;
        moto.Status = string.IsNullOrWhiteSpace(request.Status) ? moto.Status : request.Status!;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMoto(Guid id, CancellationToken cancellationToken)
    {
        var moto = await _context.Motos
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (moto is null)
        {
            return NotFound();
        }

        _context.Motos.Remove(moto);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

