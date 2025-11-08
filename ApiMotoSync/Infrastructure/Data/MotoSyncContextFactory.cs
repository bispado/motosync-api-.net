using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ApiMotoSync.Infrastructure.Data;

public class MotoSyncContextFactory : IDesignTimeDbContextFactory<MotoSyncContext>
{
    public MotoSyncContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MotoSyncContext>();

        var connectionString = Environment.GetEnvironmentVariable("OracleConnection") ??
                               "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=oracle.fiap.com.br)(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ORCL)));User Id=rm558515;Password=Fiap#2025;";

        optionsBuilder.UseOracle(connectionString);

        return new MotoSyncContext(optionsBuilder.Options);
    }
}

