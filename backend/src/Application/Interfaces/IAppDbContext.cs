using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Document> Documents { get; }
    DbSet<DocumentVersion> DocumentVersions { get; }
    DbSet<DocumentPermission> DocumentPermissions { get; }
    DbSet<AuditLog> AuditLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
