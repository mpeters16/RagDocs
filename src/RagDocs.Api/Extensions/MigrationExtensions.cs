using Microsoft.EntityFrameworkCore;
using RagDocs.Infrastructure.Persistence;

namespace RagDocs.Api.Extensions;

internal static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RagDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}