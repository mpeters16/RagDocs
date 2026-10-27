using System.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using RagDocs.Api.Background;
using RagDocs.Api.Common;
using RagDocs.Api.Endpoints;
using RagDocs.Api.Extensions;
using RagDocs.Application;
using RagDocs.Application.Abstractions;
using RagDocs.Application.Documents.Upload;
using RagDocs.Infrastructure;

// Upload limit + 1 MB for the multipart wrapping around the file.
const long MaxRequestBodyBytes = UploadLimits.MaxFileSizeBytes + (1024 * 1024);

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Layers
builder.AddInfrastructure();
builder.Services.AddApplication();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// Errors
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Upload size limits
builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = MaxRequestBodyBytes);

builder.Services.Configure<FormOptions>(options =>
    options.MultipartBodyLengthLimit = MaxRequestBodyBytes);

builder.Services.AddOpenApi();

builder.Services.AddHostedService<IngestionWorker>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapDevEndpoints();
    await app.ApplyMigrationsAsync();
}
app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapDocumentEndpoints();

app.Run();