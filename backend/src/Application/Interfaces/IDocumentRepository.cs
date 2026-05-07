using Domain.Entities;

namespace Application.Interfaces;

public interface IDocumentRepository
{
    Task<IReadOnlyCollection<Document>> GetAccessibleDocumentsAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<Document?> GetAccessibleByIdAsync(Guid documentId, Guid userId, string roleName, CancellationToken cancellationToken = default);
    Task AddAsync(Document document, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
