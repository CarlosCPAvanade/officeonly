using System;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("202605070001_InitialCreate")]
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    RoleId = table.Column<Guid>(type: "char(36)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    CurrentFilePath = table.Column<string>(type: "varchar(600)", maxLength: 600, nullable: false),
                    MimeType = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileType = table.Column<int>(type: "int", nullable: false),
                    CurrentVersionNumber = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    DocumentId = table.Column<Guid>(type: "char(36)", nullable: true),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    MetadataJson = table.Column<string>(type: "longtext", nullable: false),
                    IpAddress = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DocumentPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    DocumentId = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CanRead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanEdit = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanDelete = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentPermissions_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    DocumentId = table.Column<Guid>(type: "char(36)", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "varchar(600)", maxLength: 600, nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ChangeSummary = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentVersions_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentVersions_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_AuditLogs_DocumentId", table: "AuditLogs", column: "DocumentId");
            migrationBuilder.CreateIndex(name: "IX_AuditLogs_UserId", table: "AuditLogs", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_DocumentPermissions_DocumentId_UserId", table: "DocumentPermissions", columns: new[] { "DocumentId", "UserId" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_DocumentPermissions_UserId", table: "DocumentPermissions", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_Documents_CreatedByUserId", table: "Documents", column: "CreatedByUserId");
            migrationBuilder.CreateIndex(name: "IX_DocumentVersions_CreatedByUserId", table: "DocumentVersions", column: "CreatedByUserId");
            migrationBuilder.CreateIndex(name: "IX_DocumentVersions_DocumentId_VersionNumber", table: "DocumentVersions", columns: new[] { "DocumentId", "VersionNumber" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_Roles_Name", table: "Roles", column: "Name", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Users_Email", table: "Users", column: "Email", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Users_RoleId", table: "Users", column: "RoleId");
            migrationBuilder.CreateIndex(name: "IX_Users_UserName", table: "Users", column: "UserName", unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AuditLogs");
            migrationBuilder.DropTable(name: "DocumentPermissions");
            migrationBuilder.DropTable(name: "DocumentVersions");
            migrationBuilder.DropTable(name: "Documents");
            migrationBuilder.DropTable(name: "Users");
            migrationBuilder.DropTable(name: "Roles");
        }
    }
}
