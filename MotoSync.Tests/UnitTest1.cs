using ApiMotoSync.Domain.Entities;
using ApiMotoSync.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MotoSync.Tests;

public class MotoSyncContextTests
{
    private static MotoSyncContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<MotoSyncContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new MotoSyncContext(options);
    }

    [Fact]
    public async Task DevePersistirUsuarioComFilial()
    {
        // Arrange
        await using var context = CreateContext(nameof(DevePersistirUsuarioComFilial));

        var filial = new Filial
        {
            Id = Guid.NewGuid(),
            Nome = "Filial Centro",
            Codigo = "CT-001",
            Cidade = "Campinas",
            Estado = "SP",
            Endereco = "Rua das Flores, 200"
        };

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Maria Souza",
            Email = "maria.souza@motosync.com",
            Cargo = "Supervisora",
            FilialId = filial.Id
        };

        await context.Filiais.AddAsync(filial);
        await context.Usuarios.AddAsync(usuario);
        await context.SaveChangesAsync();

        // Act
        var persisted = await context.Usuarios
            .Include(u => u.Filial)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == usuario.Id);

        // Assert
        Assert.NotNull(persisted);
        Assert.Equal(filial.Id, persisted!.FilialId);
        Assert.NotNull(persisted.Filial);
        Assert.Equal("Filial Centro", persisted.Filial!.Nome);
    }

    [Fact]
    public async Task DeveAssociarMotoAGestorDaMesmaFilial()
    {
        // Arrange
        await using var context = CreateContext(nameof(DeveAssociarMotoAGestorDaMesmaFilial));

        var filial = new Filial
        {
            Id = Guid.NewGuid(),
            Nome = "Filial Norte",
            Codigo = "NT-002",
            Cidade = "Manaus",
            Estado = "AM",
            Endereco = "Av. das Torres, 321"
        };

        var gestor = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Carlos Silva",
            Email = "carlos.silva@motosync.com",
            Cargo = "Gestor",
            FilialId = filial.Id
        };

        var moto = new Moto
        {
            Id = Guid.NewGuid(),
            Modelo = "Yamaha Fazer 250",
            Placa = "XYZ2A34",
            Ano = 2023,
            Status = "Disponível",
            FilialId = filial.Id,
            GestorId = gestor.Id
        };

        await context.AddRangeAsync(filial, gestor, moto);
        await context.SaveChangesAsync();

        // Act
        var persisted = await context.Motos
            .Include(m => m.Gestor)
            .Include(m => m.Filial)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == moto.Id);

        // Assert
        Assert.NotNull(persisted);
        Assert.NotNull(persisted!.Gestor);
        Assert.Equal(gestor.Id, persisted.GestorId);
        Assert.NotNull(persisted.Filial);
        Assert.Equal(filial.Id, persisted.FilialId);
    }
}