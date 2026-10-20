using RagDocs.Application.Abstractions;

namespace RagDocs.UnitTests.Application.Fakes;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    // Set to true to simulate "database is down".
    public bool ThrowOnSave { get; set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCalls++;

        if (ThrowOnSave)
        {
            throw new InvalidOperationException("Database unavailable.");
        }

        return Task.FromResult(1);
    }
}