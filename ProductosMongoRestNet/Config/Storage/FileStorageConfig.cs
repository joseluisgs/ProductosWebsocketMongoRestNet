namespace ProductosMongoRestNet.Config.Storage;

/**
 * Configuración para el almacenamiento de archivos.
 */
public class FileStorageConfig
{
    public string UploadDirectory { get; set; } = "uploads";
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
    public List<string> AllowedFileTypes { get; set; } = ["image/jpeg", "image/png"];
    public bool RemoveAll { get; set; } = false;
}