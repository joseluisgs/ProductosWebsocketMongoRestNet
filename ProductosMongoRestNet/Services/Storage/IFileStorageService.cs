namespace ProductosMongoRestNet.Services.Storage;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file);
    Task<FileStream> GetFileAsync(string fileName);
    Task<bool> DeleteFileAsync(string fileName);
}