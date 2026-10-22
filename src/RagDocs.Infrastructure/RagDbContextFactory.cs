using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RagDocs.Infrastructure.Persistence;

// Used ONLY by the "dotnet ef" tool. It never runs in the real app.
public sealed class RagDbContextFactory : IDesignTimeDbContextFactory<RagDbContext>
{
    // No password: enough for "migrations add", which never connects.
    private const string FallbackConnectionString = "Host=localhost;Database=ragdocs";

    public RagDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<RagDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("ragdocs") ?? FallbackConnectionString;

        var options = new DbContextOptionsBuilder<RagDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new RagDbContext(options);
    }
}