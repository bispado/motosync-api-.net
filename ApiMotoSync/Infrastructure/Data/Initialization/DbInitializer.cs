using ApiMotoSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiMotoSync.Infrastructure.Data.Initialization;

public class DbInitializer
{
    private readonly MotoSyncContext _context;
    private readonly ILogger<DbInitializer> _logger;
    private readonly IConfiguration _configuration;

    public DbInitializer(
        MotoSyncContext context,
        ILogger<DbInitializer> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task TryInitializeAsync(CancellationToken cancellationToken = default)
    {
        var shouldInitialize = _configuration.GetValue("Database:RunInitialization", false);
        if (!shouldInitialize)
        {
            _logger.LogInformation("Inicialização do banco desativada (Database:RunInitialization = false).");
            return;
        }

        try
        {
            if (!await _context.Database.CanConnectAsync(cancellationToken))
            {
                _logger.LogWarning("Não foi possível conectar ao banco Oracle para inicialização.");
                return;
            }

            await _context.Database.EnsureCreatedAsync(cancellationToken);

            await SeedAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Inicialização do banco falhou. Execução seguirá normalmente.");
        }
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await _context.Filiais.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Dados iniciais já existem. Nenhum seed adicional será aplicado.");
            return;
        }

        var filialId = Guid.NewGuid();
        var gestorId = Guid.NewGuid();

        var filial = new Filial
        {
            Id = filialId,
            Nome = "Filial São Paulo",
            Codigo = "SP-001",
            Cidade = "São Paulo",
            Estado = "SP",
            Endereco = "Av. Paulista, 1000 - Bela Vista"
        };

        var gestor = new Usuario
        {
            Id = gestorId,
            Nome = "Gestor Padrão",
            Email = "gestor@motosync.com",
            Cargo = "Gerente Regional",
            FilialId = filialId
        };

        var moto = new Moto
        {
            Id = Guid.NewGuid(),
            Modelo = "Honda CG 160",
            Placa = "ABC1D23",
            Ano = DateTime.UtcNow.Year,
            FilialId = filialId,
            GestorId = gestorId,
            Status = "Disponível"
        };

        await _context.Filiais.AddAsync(filial, cancellationToken);
        await _context.Usuarios.AddAsync(gestor, cancellationToken);
        await _context.Motos.AddAsync(moto, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seed inicial concluído com sucesso.");
    }
}

