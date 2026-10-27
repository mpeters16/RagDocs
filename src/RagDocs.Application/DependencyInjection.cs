using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RagDocs.Application.Documents.Get;
using RagDocs.Application.Documents.List;
using RagDocs.Application.Documents.Upload;
using RagDocs.Application.Ingestion;

namespace RagDocs.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<UploadDocumentHandler>();
        services.AddScoped<GetDocumentHandler>();
        services.AddScoped<ListDocumentsHandler>();
        services.AddScoped<ProcessNextDocumentHandler>();

        services.AddSingleton<Chunker>();

        return services;
    }
}