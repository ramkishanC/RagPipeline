using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RagPipeline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRagQueryTelemetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RagQueryTelemetries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KnowledgeBaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Query = table.Column<string>(type: "text", nullable: false),
                    RewrittenQuery = table.Column<string>(type: "text", nullable: false),
                    RewriteLatencyMs = table.Column<long>(type: "bigint", nullable: false),
                    RetrievalLatencyMs = table.Column<long>(type: "bigint", nullable: false),
                    LlmLatencyMs = table.Column<long>(type: "bigint", nullable: false),
                    TotalLatencyMs = table.Column<long>(type: "bigint", nullable: false),
                    RetrievedChunkCount = table.Column<int>(type: "integer", nullable: false),
                    EstimatedPromptTokens = table.Column<int>(type: "integer", nullable: false),
                    EstimatedCompletionTokens = table.Column<int>(type: "integer", nullable: false),
                    IsNoAnswer = table.Column<bool>(type: "boolean", nullable: false),
                    CitationsValid = table.Column<bool>(type: "boolean", nullable: false),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RagQueryTelemetries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RagQueryTelemetries_KnowledgeBases_KnowledgeBaseId",
                        column: x => x.KnowledgeBaseId,
                        principalTable: "KnowledgeBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RagQueryTelemetries_KnowledgeBaseId_CreatedAt",
                table: "RagQueryTelemetries",
                columns: new[] { "KnowledgeBaseId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RagQueryTelemetries_KnowledgeBaseId_IsNoAnswer",
                table: "RagQueryTelemetries",
                columns: new[] { "KnowledgeBaseId", "IsNoAnswer" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RagQueryTelemetries");
        }
    }
}
