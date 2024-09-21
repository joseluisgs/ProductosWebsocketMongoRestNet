namespace ProductosMongoRestNet.Models;


public class Notification<T>
{
    T Data { get; set; }
    NotificationType Type { get; set; }
    DateTime CreatedAt { get; set; }

    public class NotificationType
    {
        public const string Create = "create";
        public const string Update = "update";
        public const string Delete = "delete";
    }
}