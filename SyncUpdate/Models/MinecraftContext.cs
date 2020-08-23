using Microsoft.EntityFrameworkCore;

namespace SyncUpdate.Models
{
    public partial class MinecraftContext : DbContext
    {
        public virtual DbSet<SyncUpdates> SyncUpdates { get; set; }
        public virtual DbSet<SyncVars> SyncVars { get; set; }

        public MinecraftContext(DbContextOptions<MinecraftContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<SyncVars>(e =>
            {
                e.ToTable("sync_vars");
                e.Property(t => t.Id).HasColumnType("int(11)");
                e.Property(t => t.Key)
                    .HasColumnName("key")
                    .IsRequired()
                    .HasMaxLength(45)
                    .HasDefaultValueSql("''");
                e.Property(t => t.Value)
                    .HasColumnName("value")
                    .IsRequired()
                    .HasMaxLength(90)
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<SyncUpdates>(entity =>
            {
                entity.ToTable("sync_updates");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Author)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("''");

                entity.Property(e => e.DownloadUrl)
                    .IsRequired()
                    .HasColumnName("DownloadURL")
                    .HasMaxLength(255)
                    .HasDefaultValueSql("''");

                entity.Property(e => e.LatestHash)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("''");

                entity.Property(e => e.GUID)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("''");

                entity.Property(e => e.FileName)
                    .HasMaxLength(255)
                    .HasDefaultValueSql("''");
            });
        }
    }
}
