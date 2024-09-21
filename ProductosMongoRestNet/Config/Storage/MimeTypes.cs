namespace ProductosMongoRestNet.Config.Storage;

public static class MimeTypes
{
    private static readonly Dictionary<string, string> MimeTypeMappings = new(StringComparer.InvariantCultureIgnoreCase)
    {
        { ".txt", "text/plain" },
        { ".pdf", "application/pdf" },
        { ".doc", "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".png", "image/png" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".gif", "image/gif" }
        // Añadir otros tipos MIME según sea necesario
    };

    public static string GetMimeType(string extension)
    {
        if (MimeTypeMappings.TryGetValue(extension, out var mimeType)) return mimeType;

        return "application/octet-stream"; // Valor predeterminado para tipos de archivo desconocidos
    }
}