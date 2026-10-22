using RagDocs.Api.Extensions;
using RagDocs.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Turns on logging, tracing, health checks and service discovery
// (comes from the ServiceDefaults project).
builder.AddServiceDefaults();
builder.AddInfrastructure();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrationsAsync();
}
app.UseHttpsRedirection();

// Adds /health and /alive so Aspire can check the API is OK.
app.MapDefaultEndpoints();

// Temporary: proves the API is alive.
app.MapGet("/", () => "RagDocs API is running");

app.Run();