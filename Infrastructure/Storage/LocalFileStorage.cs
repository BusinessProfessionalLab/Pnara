using Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Infrastructure.Storage;

public class LocalFileStorage(IOptions<FileStorageSettings> options) : IFileStorage
{
    public async Task<string> SaveAsync(Stream file, string directory, string fileName)
    {
        var settings = options.Value;
        var dirPath = Path.Combine(settings.BasePath, directory);
        Directory.CreateDirectory(dirPath);

        var ext = Path.GetExtension(fileName);
        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(dirPath, uniqueName);

        using var fs = File.Create(filePath);
        await file.CopyToAsync(fs);

        return $"{settings.BaseUrl}/{directory}/{uniqueName}";
    }

    public Task DeleteAsync(string url)
    {
        var settings = options.Value;
        var relativePath = url.Replace(settings.BaseUrl, "").TrimStart('/');
        var filePath = Path.Combine(settings.BasePath, relativePath);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }
}
