using ALS.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace ALS.Infrastructure.Persistence
{
    public sealed class AuditDbContext : DbContext
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

        public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var e = modelBuilder.Entity<AuditEvent>();

            e.ToTable("AuditEvents");
            e.HasKey(x => x.Id);

            e.Property(x => x.UserId).HasMaxLength(128).IsRequired();
            e.Property(x => x.ActionType).HasMaxLength(64).IsRequired();
            e.Property(x => x.EntityId).HasMaxLength(128).IsRequired();
            e.Property(x => x.MetadataJson).IsRequired();
            e.Property(x => x.IngestSource).HasMaxLength(32).IsRequired();
            e.Property(x => x.TraceId).HasMaxLength(64).IsRequired();

            e.HasIndex(x => new { x.UserId, x.Timestamp });
            e.HasIndex(x => new { x.ActionType, x.Timestamp });
            e.HasIndex(x => x.Timestamp);
        }
    }
}
