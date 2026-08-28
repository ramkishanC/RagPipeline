using Microsoft.EntityFrameworkCore;
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

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
            });

            modelBuilder.Entity<DocumentChunk>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Content)
                    .IsRequired();

                entity.HasIndex(x => new
                {
                    x.DocumentId,
                    x.ChunkIndex
                });
            });
        }
    }
}
