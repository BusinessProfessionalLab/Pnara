namespace Infrastructure.Storage;

public class FileStorageSettings
{
    public string BasePath { get; set; } = "uploads";
    public string BaseUrl { get; set; } = "/uploads";
}
