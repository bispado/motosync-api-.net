using ApiMotoSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiMotoSync.Infrastructure.Data;

public class MotoSyncContext(DbContextOptions<MotoSyncContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Filial> Filiais => Set<Filial>();
    public DbSet<Moto> Motos => Set<Moto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MotoSyncContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}

