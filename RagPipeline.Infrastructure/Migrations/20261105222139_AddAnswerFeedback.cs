using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RagPipeline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnswerFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnswerFeedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KnowledgeBaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChatMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    Question = table.Column<string>(type: "text", nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: false),
                    SourcesJson = table.Column<string>(type: "text", nullable: true),
                    IsHelpful = table.Column<bool>(type: "boolean", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnswerFeedbacks_KnowledgeBases_KnowledgeBaseId",
                        column: x => x.KnowledgeBaseId,
                        principalTable: "KnowledgeBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerFeedbacks_KnowledgeBaseId_IsHelpful",
                table: "AnswerFeedbacks",
                columns: new[] { "KnowledgeBaseId", "IsHelpful" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnswerFeedbacks");
        }
    }
}
