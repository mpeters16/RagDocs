var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddPostgres("postgres", password: postgresPassword)
    .WithImage("pgvector/pgvector", "pg17")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin(pgAdmin => pgAdmin.WithLifetime(ContainerLifetime.Persistent));

var ragDb = postgres.AddDatabase("ragdocs");

builder.AddProject<Projects.RagDocs_Api>("api")
    .WithReference(ragDb)
    .WaitFor(ragDb);

builder.Build().Run();