using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace RagDocs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChunkConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The vector type comes from the pgvector extension; it must exist before vector(768) is used.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");

            migrationBuilder.Sql(
    "DELETE FROM chunks c WHERE NOT EXISTS (SELECT 1 FROM documents d WHERE d.id = c.document_id);");
    
            migrationBuilder.AlterColumn<string>(
                name: "owner_id",
                table: "chunks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Vector>(
                name: "embedding",
                table: "chunks",
                type: "vector(768)",
                nullable: false,
                oldClrType: typeof(float[]),
                oldType: "real[]");

            migrationBuilder.CreateIndex(
                name: "ix_chunks_document_id_chunk_index",
                table: "chunks",
                columns: new[] { "document_id", "chunk_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_chunks_embedding",
                table: "chunks",
                column: "embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_chunks_owner_id",
                table: "chunks",
                column: "owner_id");

            migrationBuilder.AddForeignKey(
                name: "fk_chunks_documents_document_id",
                table: "chunks",
                column: "document_id",
                principalTable: "documents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_chunks_documents_document_id",
                table: "chunks");

            migrationBuilder.DropIndex(
                name: "ix_chunks_document_id_chunk_index",
                table: "chunks");

            migrationBuilder.DropIndex(
                name: "ix_chunks_embedding",
                table: "chunks");

            migrationBuilder.DropIndex(
                name: "ix_chunks_owner_id",
                table: "chunks");

            migrationBuilder.AlterColumn<string>(
                name: "owner_id",
                table: "chunks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

         migrationBuilder.Sql(
    "ALTER TABLE chunks ALTER COLUMN embedding TYPE vector(768) USING embedding::vector(768);");
        }
    }
}
