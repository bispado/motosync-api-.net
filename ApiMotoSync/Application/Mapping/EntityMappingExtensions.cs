using ApiMotoSync.Contracts.Responses;
using ApiMotoSync.Domain.Entities;

namespace ApiMotoSync.Application.Mapping;

public static class EntityMappingExtensions
{
    public static UsuarioResponse ToResponse(this Usuario usuario) =>
        new(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Cargo,
            usuario.FilialId,
            usuario.Filial?.Nome ?? string.Empty,
            usuario.MotosGerenciadas.Count);

    public static FilialResponse ToResponse(this Filial filial) =>
        new(
            filial.Id,
            filial.Nome,
            filial.Codigo,
            filial.Endereco,
            filial.Cidade,
            filial.Estado,
            filial.Usuarios.Count,
            filial.Motos.Count);

    public static MotoResponse ToResponse(this Moto moto) =>
        new(
            moto.Id,
            moto.Modelo,
            moto.Placa,
            moto.Ano,
            moto.Status,
            moto.FilialId,
            moto.Filial?.Nome ?? string.Empty,
            moto.GestorId,
            moto.Gestor?.Nome);
}

