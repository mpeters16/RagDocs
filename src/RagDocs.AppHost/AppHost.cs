var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddPostgres("postgres", password: postgresPassword)
    .WithImage("pgvector/pgvector", "pg17")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin(pgAdmin => pgAdmin.WithLifetime(ContainerLifetime.Persistent));

var ragDb = postgres.AddDatabase("ragdocs");

// Local AI models
var ollama = builder.AddOllama("ollama")
    //.WithGPUSupport()
    .WithDataVolume()                                   // keep downloaded models between restarts
    .WithLifetime(ContainerLifetime.Persistent);

var embeddingModel = ollama.AddModel("embedding", "nomic-embed-text");   // 768 dimensions, ~270 MB
var chatModel = ollama.AddModel("chat", "llama3.2");                     // 3B parameters, ~2 GB 

builder.AddProject<Projects.RagDocs_Api>("api")
    .WithReference(ragDb)
    .WithReference(embeddingModel)
    .WithReference(chatModel)
    .WaitFor(ragDb)
    .WaitFor(embeddingModel)
    .WaitFor(chatModel);

builder.Build().Run();