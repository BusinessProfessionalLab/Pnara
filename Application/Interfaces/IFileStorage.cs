namespace Application.Interfaces;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream file, string directory, string fileName);
    Task DeleteAsync(string url);
}
