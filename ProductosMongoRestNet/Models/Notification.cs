namespace ProductosMongoRestNet.Models;

public class Notification<T>
{
    public enum NotificationType
    {
        Create,
        Update,
        Delete
    }

    public T Data { get; set; }
    public string Type { get; set; }
    public DateTime CreatedAt { get; set; }
}