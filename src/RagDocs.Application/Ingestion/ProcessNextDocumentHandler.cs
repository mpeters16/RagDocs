using Microsoft.Extensions.Logging;
using RagDocs.Application.Abstractions;
using RagDocs.Domain.Chunks;
using RagDocs.Domain.Common;
using RagDocs.Domain.Documents;

namespace RagDocs.Application.Ingestion;

public sealed class ProcessNextDocumentHandler(
    IDocumentRepository documentRepository,
    IChunkRepository chunkRepository,
    IFileStore fileStore,
    ITextExtractor textExtractor,
    Chunker chunker,
    IEmbeddingService embeddingService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<ProcessNextDocumentHandler> logger)
{
    private const int EmbeddingBatchSize = 32;

    // Returns true if a document was processed (so the worker checks again straight away).
    public async Task<bool> HandleAsync(CancellationToken cancellationToken)
    {
        // 1. Take the next ticket
        var document = await documentRepository.ClaimNextPendingAsync(cancellationToken);

        if (document is null)
        {
            return false;
        }

        logger.LogInformation("Processing document {DocumentId}", document.Id);

        // 2–5. Open, extract, chunk, embed (nothing is saved yet)
        Result<IReadOnlyList<Chunk>> result;

        try
        {
            result = await BuildChunksAsync(document, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Unexpected (e.g. Ollama down): fail this document, keep the worker alive.
            logger.LogError(exception, "Processing failed for document {DocumentId}", document.Id);
            result = Error.Failure("Processing.Unexpected", "Processing failed unexpectedly. Please try again later.");
        }

        var now = timeProvider.GetUtcNow();

        // 6a. Something went wrong → Failed, with a reason
        if (result.IsFailure)
        {
            EnsureSuccess(document.MarkFailed(result.Error.Description, now));
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogWarning("Document {DocumentId} failed: {Code}", document.Id, result.Error.Code);
            return true;
        }

        // 6b. Everything worked → replace the chunks and mark Ready
        await chunkRepository.DeleteForDocumentAsync(document.Id, cancellationToken);
        chunkRepository.AddRange(result.Value);
        EnsureSuccess(document.MarkReady(result.Value.Count, now));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Document {DocumentId} ready with {ChunkCount} chunks", document.Id, result.Value.Count);

        return true;
    }

    private async Task<Result<IReadOnlyList<Chunk>>> BuildChunksAsync(
        Document document,
        CancellationToken cancellationToken)
    {
        // 2. Open the file
        await using var content = await fileStore.OpenReadAsync(document.StoragePath, cancellationToken);

        // 3. Extract the text
        var pages = await textExtractor.ExtractAsync(content, document.ContentType, cancellationToken);

        if (pages.IsFailure)
        {
            return pages.Error;
        }

        // 4. Split into chunks
        var textChunks = chunker.Split(pages.Value, ChunkingOptions.Default);

        if (textChunks.Count == 0)
        {
            return ExtractionErrors.NoText;
        }

        // 5. Embed in batches and build Chunk entities
        var chunks = new List<Chunk>(textChunks.Count);

        foreach (var batch in textChunks.Chunk(EmbeddingBatchSize))
        {
            var vectors = await embeddingService.EmbedAsync(
                batch.Select(c => c.Text).ToList(),
                EmbeddingPurpose.Document,
                cancellationToken);

            for (var i = 0; i < batch.Length; i++)
            {
                var chunk = Chunk.Create(
                    document.Id,
                    document.OwnerId,
                    batch[i].Index,
                    batch[i].Text,
                    batch[i].PageNumber,
                    batch[i].TokenCount,
                    vectors[i]);

                if (chunk.IsFailure)
                {
                    return chunk.Error;
                }

                chunks.Add(chunk.Value);
            }
        }

        return Result<IReadOnlyList<Chunk>>.Success(chunks);
    }

    // The document was just claimed (Processing), so these can't fail.
    // If one does, it's a bug — fail loudly.
    private static void EnsureSuccess(Result result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Unexpected state transition failure: {result.Error.Code}");
        }
    }
}