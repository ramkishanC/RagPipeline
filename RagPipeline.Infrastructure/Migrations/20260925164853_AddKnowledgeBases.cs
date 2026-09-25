using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RagPipeline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKnowledgeBases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KnowledgeBaseId",
                table: "Documents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "RagUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalUserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RagUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeBases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RagUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeBases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnowledgeBases_RagUsers_RagUserId",
                        column: x => x.RagUserId,
                        principalTable: "RagUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_KnowledgeBaseId",
                table: "Documents",
                column: "KnowledgeBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeBases_RagUserId_Name",
                table: "KnowledgeBases",
                columns: new[] { "RagUserId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_RagUsers_ExternalUserId",
                table: "RagUsers",
                column: "ExternalUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_KnowledgeBases_KnowledgeBaseId",
                table: "Documents",
                column: "KnowledgeBaseId",
                principalTable: "KnowledgeBases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_KnowledgeBases_KnowledgeBaseId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "KnowledgeBases");

            migrationBuilder.DropTable(
                name: "RagUsers");

            migrationBuilder.DropIndex(
                name: "IX_Documents_KnowledgeBaseId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "KnowledgeBaseId",
                table: "Documents");
        }
    }
}
