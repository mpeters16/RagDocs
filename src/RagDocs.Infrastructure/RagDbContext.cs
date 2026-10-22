using Microsoft.EntityFrameworkCore;
using RagDocs.Application.Abstractions;
using RagDocs.Domain.Documents;

namespace RagDocs.Infrastructure.Persistence;

public sealed class RagDbContext(DbContextOptions<RagDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RagDbContext).Assembly);
    }
}