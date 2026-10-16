using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NEHGSWebApiCore.Domain.Entities;

namespace NEHGSWebApiCore.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
        public DbSet<ImportFile> ImportFiles { get; set; }
        public DbSet<ImportSerializedMapping> ImportSerializedMappings { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<RecordRole> RecordRoles { get; set; }
        public DbSet<CollectionAttribute> Attributes { get; set; }
        public DbSet<ExtendedAttribute> ExtendedAttributes { get; set; }
        public DbSet<ErrorCheckResult> ErrorCheckResults { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(builder);

            // ImportFile mapping
            builder.Entity<ImportFile>(entity =>
            {
                entity.ToTable("ImportFiles");
                entity.HasKey(e => e.ImportFileId);
                entity.Property(e => e.SourceFilename).HasMaxLength(500);
                entity.Property(e => e.ImportedBy).HasMaxLength(200);
                entity.Property(e => e.ProcessMessage).HasMaxLength(2000);
                entity.Property(e => e.ImportName).HasMaxLength(500);

                entity.HasOne(e => e.ImportSerializedMapping)
                    .WithMany(m => m.ImportFiles)
                    .HasForeignKey(e => e.ImportSerializeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Collection)
                    .WithMany(c => c.ImportFiles)
                    .HasForeignKey(e => e.TargetCollectionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ImportSerializedMapping mapping
            builder.Entity<ImportSerializedMapping>(entity =>
            {
                entity.ToTable("ImportSerializedMappings");
                entity.HasKey(e => e.ImportSerializeId);
                entity.Property(e => e.MappingName).HasMaxLength(500);
                entity.Property(e => e.SerializedMappings).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CreatedBy).HasMaxLength(200);
                entity.Property(e => e.UpdatedBy).HasMaxLength(200);
            });

            // Collection mapping
            builder.Entity<Collection>(entity =>
            {
                entity.ToTable("Collections");
                entity.HasKey(e => e.CollectionId);
                entity.Property(e => e.Name).HasMaxLength(500);
                entity.Property(e => e.ShortDescription).HasMaxLength(1000);
                entity.Property(e => e.ImageUrl).HasMaxLength(1000);
                entity.Property(e => e.ImageTargetLink).HasMaxLength(1000);
                entity.Property(e => e.ErrorReportMessage).HasMaxLength(2000);
            });

            // RecordRole mapping
            builder.Entity<RecordRole>(entity =>
            {
                entity.ToTable("RecordRoles");
                entity.HasKey(e => e.RecordRoleId);
                entity.Property(e => e.Name).HasMaxLength(200);
            });

            // ExtendedAttribute mapping
            builder.Entity<ExtendedAttribute>(entity =>
            {
                entity.ToTable("ExtendedAttributes");
                entity.HasKey(e => e.ExtendedAttributeId);
                entity.Property(e => e.Name).HasMaxLength(200);
            });

            // Attribute mapping
            builder.Entity<CollectionAttribute>(entity =>
            {
                entity.ToTable("Attributes");
                entity.HasKey(e => e.AttributeId);
                entity.HasOne(a => a.ExtendedAttribute).WithMany().HasForeignKey(a => a.ExtendedAttributeId).OnDelete(DeleteBehavior.Restrict);
            });

            // ErrorCheckResult mapping
            builder.Entity<ErrorCheckResult>(entity =>
            {
                entity.ToTable("ErrorCheckResults");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProgressId).HasMaxLength(200);
                entity.Property(e => e.SerializedErrors).HasColumnType("nvarchar(max)");
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedDate = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
