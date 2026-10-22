using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RagDocs.Application.Abstractions;
using RagDocs.Infrastructure.Persistence;
using RagDocs.Infrastructure.Persistence.Repositories;

namespace RagDocs.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<RagDbContext>(options =>
            options
                .UseNpgsql(builder.Configuration.GetConnectionString("ragdocs"))
                .UseSnakeCaseNamingConvention());

        // Aspire extras: health check, connection retries, database calls in the dashboard traces.
        builder.EnrichNpgsqlDbContext<RagDbContext>();

        // IUnitOfWork must be THE SAME RagDbContext the repository uses.
        builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<RagDbContext>());

        builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

        return builder;
    }
}