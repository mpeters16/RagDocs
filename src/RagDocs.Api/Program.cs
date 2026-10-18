var builder = WebApplication.CreateBuilder(args);

// Turns on logging, tracing, health checks and service discovery
// (comes from the ServiceDefaults project).
builder.AddServiceDefaults();

// Creates a description of your endpoints for tools and docs.
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Shows that description at /openapi/v1.json while developing.
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Adds /health and /alive so Aspire can check the API is OK.
app.MapDefaultEndpoints();

// Temporary: proves the API is alive.
app.MapGet("/", () => "RagDocs API is running");

app.Run();