using System;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.5");

            modelBuilder.Entity("Domain.Entities.AuditLog", b =>
            {
                b.Property<Guid>("Id").HasColumnType("char(36)");
                b.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime(6)");
                b.Property<string>("Description").IsRequired().HasMaxLength(500).HasColumnType("varchar(500)");
                b.Property<Guid?>("DocumentId").HasColumnType("char(36)");
                b.Property<string>("IpAddress").IsRequired().HasMaxLength(100).HasColumnType("varchar(100)");
                b.Property<string>("MetadataJson").IsRequired().HasColumnType("longtext");
                b.Property<int>("ActionType").HasColumnType("int");
                b.Property<Guid?>("UserId").HasColumnType("char(36)");
                b.HasKey("Id");
                b.HasIndex("DocumentId");
                b.HasIndex("UserId");
                b.ToTable("AuditLogs");
            });

            modelBuilder.Entity("Domain.Entities.Document", b =>
            {
                b.Property<Guid>("Id").HasColumnType("char(36)");
                b.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime(6)");
                b.Property<Guid>("CreatedByUserId").HasColumnType("char(36)");
                b.Property<string>("CurrentFilePath").IsRequired().HasMaxLength(600).HasColumnType("varchar(600)");
                b.Property<int>("CurrentVersionNumber").HasColumnType("int");
                b.Property<int>("FileType").HasColumnType("int");
                b.Property<bool>("IsDeleted").HasColumnType("tinyint(1)");
                b.Property<string>("MimeType").IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
                b.Property<string>("OriginalFileName").IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
                b.Property<long>("SizeInBytes").HasColumnType("bigint");
                b.Property<string>("Title").IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
                b.Property<DateTime>("UpdatedAtUtc").HasColumnType("datetime(6)");
                b.HasKey("Id");
                b.HasIndex("CreatedByUserId");
                b.ToTable("Documents");
            });

            modelBuilder.Entity("Domain.Entities.DocumentPermission", b =>
            {
                b.Property<Guid>("Id").HasColumnType("char(36)");
                b.Property<bool>("CanDelete").HasColumnType("tinyint(1)");
                b.Property<bool>("CanEdit").HasColumnType("tinyint(1)");
                b.Property<bool>("CanRead").HasColumnType("tinyint(1)");
                b.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime(6)");
                b.Property<Guid>("DocumentId").HasColumnType("char(36)");
                b.Property<Guid>("UserId").HasColumnType("char(36)");
                b.HasKey("Id");
                b.HasIndex("DocumentId", "UserId").IsUnique();
                b.HasIndex("UserId");
                b.ToTable("DocumentPermissions");
            });

            modelBuilder.Entity("Domain.Entities.DocumentVersion", b =>
            {
                b.Property<Guid>("Id").HasColumnType("char(36)");
                b.Property<string>("ChangeSummary").IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
                b.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime(6)");
                b.Property<Guid>("CreatedByUserId").HasColumnType("char(36)");
                b.Property<Guid>("DocumentId").HasColumnType("char(36)");
                b.Property<string>("FilePath").IsRequired().HasMaxLength(600).HasColumnType("varchar(600)");
                b.Property<long>("SizeInBytes").HasColumnType("bigint");
                b.Property<int>("VersionNumber").HasColumnType("int");
                b.HasKey("Id");
                b.HasIndex("CreatedByUserId");
                b.HasIndex("DocumentId", "VersionNumber").IsUnique();
                b.ToTable("DocumentVersions");
            });

            modelBuilder.Entity("Domain.Entities.Role", b =>
            {
                b.Property<Guid>("Id").HasColumnType("char(36)");
                b.Property<string>("Name").IsRequired().HasMaxLength(50).HasColumnType("varchar(50)");
                b.HasKey("Id");
                b.HasIndex("Name").IsUnique();
                b.ToTable("Roles");
            });

            modelBuilder.Entity("Domain.Entities.User", b =>
            {
                b.Property<Guid>("Id").HasColumnType("char(36)");
                b.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime(6)");
                b.Property<string>("Email").IsRequired().HasMaxLength(200).HasColumnType("varchar(200)");
                b.Property<bool>("IsActive").HasColumnType("tinyint(1)");
                b.Property<string>("PasswordHash").IsRequired().HasMaxLength(500).HasColumnType("varchar(500)");
                b.Property<Guid>("RoleId").HasColumnType("char(36)");
                b.Property<string>("UserName").IsRequired().HasMaxLength(100).HasColumnType("varchar(100)");
                b.HasKey("Id");
                b.HasIndex("Email").IsUnique();
                b.HasIndex("RoleId");
                b.HasIndex("UserName").IsUnique();
                b.ToTable("Users");
            });

            modelBuilder.Entity("Domain.Entities.AuditLog", b =>
            {
                b.HasOne("Domain.Entities.Document", "Document")
                    .WithMany("AuditLogs")
                    .HasForeignKey("DocumentId")
                    .OnDelete(DeleteBehavior.SetNull);

                b.HasOne("Domain.Entities.User", "User")
                    .WithMany("AuditLogs")
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity("Domain.Entities.Document", b =>
            {
                b.HasOne("Domain.Entities.User", "CreatedByUser")
                    .WithMany("CreatedDocuments")
                    .HasForeignKey("CreatedByUserId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });

            modelBuilder.Entity("Domain.Entities.DocumentPermission", b =>
            {
                b.HasOne("Domain.Entities.Document", "Document")
                    .WithMany("Permissions")
                    .HasForeignKey("DocumentId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Domain.Entities.User", "User")
                    .WithMany("DocumentPermissions")
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Domain.Entities.DocumentVersion", b =>
            {
                b.HasOne("Domain.Entities.User", "CreatedByUser")
                    .WithMany("CreatedVersions")
                    .HasForeignKey("CreatedByUserId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("Domain.Entities.Document", "Document")
                    .WithMany("Versions")
                    .HasForeignKey("DocumentId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Domain.Entities.User", b =>
            {
                b.HasOne("Domain.Entities.Role", "Role")
                    .WithMany("Users")
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });
        }
    }
}
