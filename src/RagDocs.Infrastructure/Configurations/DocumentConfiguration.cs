using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RagDocs.Domain.Documents;

namespace RagDocs.Infrastructure.Persistence.Configurations;

internal sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");

        // ---------- Key ----------

        builder.HasKey(d => d.Id);

        // The Domain creates ids (Guid.CreateVersion7); the database must not.
        builder.Property(d => d.Id).ValueGeneratedNever();

        // ---------- Columns ----------

        builder.Property(d => d.OwnerId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.FileName)
            .HasMaxLength(Document.MaxFileNameLength)
            .IsRequired();

        builder.Property(d => d.ContentType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.StoragePath)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(d => d.ContentHash)
            .HasMaxLength(64)
            .IsFixedLength()
            .IsRequired();

        builder.Property(d => d.SizeBytes)
            .IsRequired();

        // Stored as text ("Pending", "Ready") instead of 0, 1, 2.
        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(d => d.FailureReason)
            .HasMaxLength(2000);

        builder.Property(d => d.ChunkCount)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.ProcessedAt);

        // ---------- Indexes ----------

        // "My documents, newest first" (ListAsync).
        builder.HasIndex(d => new { d.OwnerId, d.CreatedAt });

        // One copy of each file per user. Also blocks two identical
        // uploads arriving at the same moment
        builder.HasIndex(d => new { d.OwnerId, d.ContentHash })
            .IsUnique();

        // The ingestion worker looks for Pending documents.
        builder.HasIndex(d => d.Status);
    }
}