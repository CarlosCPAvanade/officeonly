using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _dbContext;

    public DocumentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Document>> GetAccessibleDocumentsAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        var query = IncludeDocumentGraph().Where(x => !x.IsDeleted);
        if (!roleName.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.Permissions.Any(p => p.UserId == userId && p.CanRead));
        }

        return await query
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Document?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return IncludeDocumentGraph().FirstOrDefaultAsync(x => x.Id == documentId, cancellationToken);
    }

    public Task<Document?> GetAccessibleByIdAsync(Guid documentId, Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        var query = IncludeDocumentGraph().Where(x => x.Id == documentId && !x.IsDeleted);
        if (!roleName.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.Permissions.Any(p => p.UserId == userId && p.CanRead));
        }

        return query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _dbContext.Documents.AddAsync(document, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Document> IncludeDocumentGraph()
    {
        return _dbContext.Documents
            .Include(x => x.CreatedByUser)
            .Include(x => x.Permissions)
            .Include(x => x.Versions)
                .ThenInclude(x => x.CreatedByUser);
    }
}
