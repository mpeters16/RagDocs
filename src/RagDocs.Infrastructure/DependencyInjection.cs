using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RagDocs.Application.Abstractions;
using RagDocs.Infrastructure.Extraction;
using RagDocs.Infrastructure.Persistence;
using RagDocs.Infrastructure.Persistence.Repositories;
using RagDocs.Infrastructure.Storage;
using Microsoft.Extensions.AI;
using OllamaSharp;
using RagDocs.Infrastructure.AI;

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

         // File storage
        builder.Services.AddOptions<StorageOptions>()
            .BindConfiguration(StorageOptions.SectionName)
            .Validate(o => !string.IsNullOrWhiteSpace(o.RootPath), "Storage:RootPath is required.")
            .ValidateOnStart();

        builder.Services.AddSingleton<IFileStore, LocalFileStore>();
        
        // Text extraction and token counting
        builder.Services.AddSingleton<ITextExtractor, TextExtractor>();
        builder.Services.AddSingleton<ITokenCounter, CharacterTokenCounter>();

        //AI
        AddAi(builder);
        
        return builder;
    }

        private static void AddAi(IHostApplicationBuilder builder)
    {
        builder.Services.AddOptions<AiOptions>()
            .BindConfiguration(AiOptions.SectionName);

        var embedding = OllamaConnection.Parse(builder.Configuration.GetConnectionString("embedding"), "embedding");
        var chat = OllamaConnection.Parse(builder.Configuration.GetConnectionString("chat"), "chat");

        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
            new OllamaApiClient(embedding.Endpoint, embedding.Model);

        IChatClient chatClient = new OllamaApiClient(chat.Endpoint, chat.Model);

        builder.Services.AddEmbeddingGenerator(embeddingGenerator);
        builder.Services.AddChatClient(chatClient);   // used in session 6

        builder.Services.AddSingleton<IEmbeddingService, EmbeddingService>();
    }
}