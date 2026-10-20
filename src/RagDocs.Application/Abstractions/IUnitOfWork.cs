namespace RagDocs.Application.Abstractions;

public interface IUnitOfWork
{
    // Writes all tracked changes to the database in one transaction.
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}