using Microsoft.Extensions.Options;
using ProductosMongoRestNet.Config.Storage;
using ProductosMongoRestNet.Exceptions.FileStorage;

namespace ProductosMongoRestNet.Services.Storage;

public class FileStorageService : IFileStorageService
{
    private readonly FileStorageConfig _fileStorageConfig;
    private readonly ILogger _logger;

    public FileStorageService(IOptions<FileStorageConfig> fileStorageConfig, ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _fileStorageConfig = fileStorageConfig.Value;
    }

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        _logger.LogInformation($"Saving file: {file.FileName}");

        // Comprobamos el tamaño del fichero
        if (file.Length > _fileStorageConfig.MaxFileSize)
            throw new FileStorageException("El tamaño del fichero excede el máximo permitido.");

        // Comprobamos la extensión del fichero
        var fileExtension = Path.GetExtension(file.FileName);
        if (!_fileStorageConfig.AllowedFileTypes.Contains(fileExtension))
            throw new FileStorageException("Tipo de fichero no permitido.");

        // Creamos el directorio de subida si no existe, debería estar hecho antes, pero por si acaso
        var uploadPath = Path.Combine(_fileStorageConfig.UploadDirectory);
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        // Guardamos el fichero
        var fileName = Guid.NewGuid() + fileExtension;
        var filePath = Path.Combine(uploadPath, fileName);

        // Using es el equivalente a usar un bloque try-finally para asegurarnos de que el recurso se libera correctamente
        await using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        _logger.LogInformation($"File saved: {fileName}");
        return fileName;
    }

    public async Task<FileStream> GetFileAsync(string fileName)
    {
        _logger.LogInformation($"Getting file: {fileName}");
        try
        {
            var filePath = Path.Combine(_fileStorageConfig.UploadDirectory, fileName);

            // Comprobamos si el fichero existe
            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"File not found: {filePath}");
                // throw new FileNotFoundException($"File not found: {fileName}");
                throw new FileStorageException($"File not found: {fileName}");
            }

            // Si todo va bien, devuelve el stream del fichero
            _logger.LogInformation($"File found: {filePath}");
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file");
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        _logger.LogInformation($"Deleting file: {fileName}");
        try
        {
            var filePath = Path.Combine(_fileStorageConfig.UploadDirectory, fileName);

            // Comprobamos si el fichero existe
            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"File not found: {filePath}");
                return false;
            }

            // Si existe, lo borramos
            File.Delete(filePath);
            _logger.LogInformation($"File deleted: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            throw;
        }
    }
}