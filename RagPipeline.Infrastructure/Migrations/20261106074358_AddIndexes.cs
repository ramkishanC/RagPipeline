using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RagPipeline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_KnowledgeBaseId",
                table: "Documents");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_KnowledgeBaseId_Category",
                table: "Documents",
                columns: new[] { "KnowledgeBaseId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_KnowledgeBaseId_Status",
                table: "Documents",
                columns: new[] { "KnowledgeBaseId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentChunks_Embedding",
                table: "DocumentChunks",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_KnowledgeBaseId_Category",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_KnowledgeBaseId_Status",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_DocumentChunks_Embedding",
                table: "DocumentChunks");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_KnowledgeBaseId",
                table: "Documents",
                column: "KnowledgeBaseId");
        }
    }
}
