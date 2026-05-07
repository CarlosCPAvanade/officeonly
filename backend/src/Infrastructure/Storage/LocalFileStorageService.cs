using Application.Interfaces;
using Application.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IOptions<StorageOptions> storageOptions)
    {
        _rootPath = Path.GetFullPath(storageOptions.Value.RootPath);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream stream, string container, string fileName, CancellationToken cancellationToken = default)
    {
        var relativePath = Path.Combine(container, fileName).Replace("\\", "/");
        var absolutePath = GetAbsolutePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        await using var target = File.Create(absolutePath);
        await stream.CopyToAsync(target, cancellationToken);
        await target.FlushAsync(cancellationToken);
        return relativePath;
    }

    public async Task ReplaceAsync(string relativePath, Stream stream, CancellationToken cancellationToken = default)
    {
        var absolutePath = GetAbsolutePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        await using var target = File.Create(absolutePath);
        await stream.CopyToAsync(target, cancellationToken);
        await target.FlushAsync(cancellationToken);
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = GetAbsolutePath(relativePath);
        Stream stream = File.OpenRead(absolutePath);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = GetAbsolutePath(relativePath);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }

    public Task<long> GetSizeAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = GetAbsolutePath(relativePath);
        var info = new FileInfo(absolutePath);
        return Task.FromResult(info.Length);
    }

    public Task CopyAsync(string sourceRelativePath, string targetRelativePath, CancellationToken cancellationToken = default)
    {
        var sourcePath = GetAbsolutePath(sourceRelativePath);
        var targetPath = GetAbsolutePath(targetRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        File.Copy(sourcePath, targetPath, true);
        return Task.CompletedTask;
    }

    private string GetAbsolutePath(string relativePath)
    {
        return Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
