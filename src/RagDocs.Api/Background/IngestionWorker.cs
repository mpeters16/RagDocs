using RagDocs.Application.Abstractions;
using RagDocs.Application.Ingestion;

namespace RagDocs.Api.Background;

internal sealed class IngestionWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<IngestionWorker> logger) : BackgroundService
{
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Once, at startup: documents left half-done by a crash go back to Pending.
        await ResetStuckDocumentsAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var processedOne = false;

            try
            {
                // A fresh scope per job: fresh RagDbContext, fresh handler.
                await using var scope = scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<ProcessNextDocumentHandler>();

                processedOne = await handler.HandleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;   // the app is shutting down
            }
            catch (Exception exception)
            {
                // e.g. database briefly unavailable: log, wait, try again.
                logger.LogError(exception, "Ingestion loop error");
            }

            if (!processedOne)
            {
                try
                {
                    await Task.Delay(IdleDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;   // shutting down while waiting
                }
            }
        }
    }

    private async Task ResetStuckDocumentsAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var documents = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();

        var reset = await documents.ResetStuckProcessingAsync(stoppingToken);

        if (reset > 0)
        {
            logger.LogWarning("Reset {Count} documents stuck in Processing", reset);
        }
    }
}