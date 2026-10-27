using Microsoft.EntityFrameworkCore;
using RagDocs.Application.Abstractions;
using RagDocs.Domain.Chunks;
using RagDocs.Domain.Documents;

namespace RagDocs.Infrastructure.Persistence;

public sealed class RagDbContext(DbContextOptions<RagDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Document> Documents => Set<Document>();

    public DbSet<Chunk> Chunks => Set<Chunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RagDbContext).Assembly);
    }
}