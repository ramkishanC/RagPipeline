using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using RagPipeline.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Infrastructure.Persistence
{
    public class RagDbContext : DbContext
    {
        public RagDbContext(DbContextOptions<RagDbContext> options)
            : base(options)
        {
        }

        public DbSet<Document> Documents => Set<Document>();

        public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

        public DbSet<RagUser> RagUsers => Set<RagUser>();

        public DbSet<KnowledgeBase> KnowledgeBases => Set<KnowledgeBase>();

        public DbSet<Conversation> Conversations { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<AnswerFeedback> AnswerFeedbacks { get; set; }

        public DbSet<RagQueryTelemetry> RagQueryTelemetries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresExtension("vector");

            modelBuilder.Entity<RagUser>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ExternalUserId)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasIndex(x => x.ExternalUserId)
                    .IsUnique();

                entity.HasMany(x => x.KnowledgeBases)
                    .WithOne(x => x.RagUser)
                    .HasForeignKey(x => x.RagUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<KnowledgeBase>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasMany(x => x.Documents)
                    .WithOne(x => x.KnowledgeBase)
                    .HasForeignKey(x => x.KnowledgeBaseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => new { x.RagUserId, x.Name });
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.FileName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(x => x.ContentType)
                    .HasMaxLength(200);

                entity.HasMany(x => x.Chunks)
                    .WithOne(x => x.Document)
                    .HasForeignKey(x => x.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => new { x.KnowledgeBaseId, x.Status });
                entity.HasIndex(x => new { x.KnowledgeBaseId, x.Category });
            });

            modelBuilder.Entity<DocumentChunk>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Content)
                    .IsRequired();

                entity.Property(x => x.Embedding)
                    .HasColumnType("vector(768)");

                entity.Property<NpgsqlTsVector>("SearchVector")
                    .IsGeneratedTsVectorColumn("english", nameof(DocumentChunk.Content));

                entity.HasIndex("SearchVector")
                    .HasMethod("GIN");

                entity.HasIndex(x => new { x.DocumentId, x.ChunkIndex });

                entity.HasIndex(x => x.Embedding)
                    .HasMethod("hnsw")
                    .HasOperators("vector_cosine_ops");
            });

            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.KnowledgeBase)
                    .WithMany(kb => kb.Conversations)
                    .HasForeignKey(c => c.KnowledgeBaseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasOne(m => m.Conversation)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(m => m.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AnswerFeedback>(entity =>
            {
                entity.HasKey(f => f.Id);

                entity.Property(f => f.Question)
                    .IsRequired();

                entity.Property(f => f.Answer)
                    .IsRequired();

                entity.HasOne(f => f.KnowledgeBase)
                    .WithMany(kb => kb.Feedbacks)
                    .HasForeignKey(f => f.KnowledgeBaseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(f => new { f.KnowledgeBaseId, f.IsHelpful });
            });

            modelBuilder.Entity<RagQueryTelemetry>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Query)
                    .IsRequired();

                entity.HasOne(t => t.KnowledgeBase)
                    .WithMany(kb => kb.QueryTelemetries)
                    .HasForeignKey(t => t.KnowledgeBaseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(t => new { t.KnowledgeBaseId, t.CreatedAt });
                entity.HasIndex(t => new { t.KnowledgeBaseId, t.IsNoAnswer });
            });
        }
    }
}
