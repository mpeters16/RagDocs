using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pgvector;
using RagDocs.Application.Abstractions;
using RagDocs.Application.Search;

namespace RagDocs.Infrastructure.Search;

internal sealed class VectorChunkSearch(RagDbContext dbContext) : IChunkSearch
{
    private const string Sql = """
        SELECT c.id,
               c.document_id,
               d.file_name,
               c.chunk_index,
               c.page_number,
               c.content,
               1 - (c.embedding <=> @embedding) AS score
        FROM chunks c
        JOIN documents d ON d.id = c.document_id
        WHERE c.owner_id = @owner_id
          AND d.status = 'Ready'
        ORDER BY c.embedding <=> @embedding
        LIMIT @top_k
        """;

    public async Task<IReadOnlyList<RetrievedChunk>> SearchAsync(
        float[] queryEmbedding,
        string ownerId,
        int topK,
        CancellationToken cancellationToken)
    {
        // Use the DbContext's own connection (same connection string, same pgvector setup).
        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await using var command = new NpgsqlCommand(Sql, connection);
            command.Parameters.AddWithValue("embedding", new Vector(queryEmbedding));
            command.Parameters.AddWithValue("owner_id", ownerId);
            command.Parameters.AddWithValue("top_k", topK);

            var results = new List<RetrievedChunk>(topK);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(new RetrievedChunk(
                    ChunkId: reader.GetGuid(0),
                    DocumentId: reader.GetGuid(1),
                    FileName: reader.GetString(2),
                    ChunkIndex: reader.GetInt32(3),
                    PageNumber: reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    Content: reader.GetString(5),
                    Score: reader.GetDouble(6)));
            }

            return results;
        }
        finally
        {
            await dbContext.Database.CloseConnectionAsync();
        }
    }
}