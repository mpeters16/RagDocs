using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;
using RagDocs.Domain.Chunks;
using RagDocs.Domain.Documents;
using RagDocs.Infrastructure.AI;

namespace RagDocs.Infrastructure.Configurations;   

internal sealed class ChunkConfiguration : IEntityTypeConfiguration<Chunk>
{
    public void Configure(EntityTypeBuilder<Chunk> builder)
    {
        builder.ToTable("chunks");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.OwnerId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Content)
            .IsRequired();

        // Domain uses float[]; the database column is pgvector's vector(768).
        builder.Property(c => c.Embedding)
            .HasConversion(
                array => new Vector(array),
                vector => vector.ToArray(),
                new ValueComparer<float[]>(
                    (a, b) => a!.SequenceEqual(b!),
                    array => array.Aggregate(0, (hash, value) => HashCode.Combine(hash, value)),
                    array => array.ToArray()))
            .HasColumnType($"vector({EmbeddingSettings.Dimensions})")
            .IsRequired();

        // Deleting a document deletes its chunks.
        builder.HasOne<Document>()
            .WithMany()
            .HasForeignKey(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Each position appears once per document.
        builder.HasIndex(c => new { c.DocumentId, c.ChunkIndex }).IsUnique();

        // Search always filters by owner.
        builder.HasIndex(c => c.OwnerId);

        // Fast nearest-neighbour search by cosine distance.
        builder.HasIndex(c => c.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops");
    }
}