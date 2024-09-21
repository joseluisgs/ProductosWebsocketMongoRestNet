namespace ProductosMongoRestNet.Exceptions.FileStorage;

/**
 * Excepción para el almacenamiento de archivos.
 */
public class FileStorageException : Exception
{
    public FileStorageException(string message) : base(message)
    {
    }

    public FileStorageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}