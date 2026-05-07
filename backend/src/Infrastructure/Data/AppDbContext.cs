using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<DocumentPermission> DocumentPermissions => Set<DocumentPermission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.HasIndex(x => x.UserName).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Documents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(255).IsRequired();
            entity.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.CurrentFilePath).HasMaxLength(600).IsRequired();
            entity.Property(x => x.MimeType).HasMaxLength(255).IsRequired();
            entity.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CreatedDocuments)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DocumentVersion>(entity =>
        {
            entity.ToTable("DocumentVersions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FilePath).HasMaxLength(600).IsRequired();
            entity.Property(x => x.ChangeSummary).HasMaxLength(255).IsRequired();
            entity.HasIndex(x => new { x.DocumentId, x.VersionNumber }).IsUnique();
            entity.HasOne(x => x.Document)
                .WithMany(x => x.Versions)
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CreatedVersions)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DocumentPermission>(entity =>
        {
            entity.ToTable("DocumentPermissions");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.DocumentId, x.UserId }).IsUnique();
            entity.HasOne(x => x.Document)
                .WithMany(x => x.Permissions)
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.User)
                .WithMany(x => x.DocumentPermissions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
            entity.Property(x => x.IpAddress).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MetadataJson).HasColumnType("longtext");
            entity.HasOne(x => x.User)
                .WithMany(x => x.AuditLogs)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Document)
                .WithMany(x => x.AuditLogs)
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
