namespace Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream stream, string container, string fileName, CancellationToken cancellationToken = default);
    Task ReplaceAsync(string relativePath, Stream stream, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
    Task<long> GetSizeAsync(string relativePath, CancellationToken cancellationToken = default);
    Task CopyAsync(string sourceRelativePath, string targetRelativePath, CancellationToken cancellationToken = default);
}
