using ApiMotoSync.Infrastructure.Data;
using ApiMotoSync.Infrastructure.Data.Initialization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<MotoSyncContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("OracleConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Connection string 'OracleConnection' não foi configurada.");
    }

    options.UseOracle(connectionString);
});

builder.Services.AddScoped<DbInitializer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.MapControllers();

app.MapGet("/wellcome", () => Results.Text("Rota adicionada"))
    .WithName("Wellcome")
    .WithTags("Health")
    .Produces<string>(StatusCodes.Status200OK);

await InitializeDatabaseAsync(app);

await app.RunAsync();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await initializer.TryInitializeAsync();
}
