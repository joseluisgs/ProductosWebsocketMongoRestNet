using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProductosMongoRestNet.Models;

public class Book
{
    [BsonId] // Esto indica que el campo es el identificador de la colección
    [BsonRepresentation(BsonType.ObjectId)] // Esto indica que el campo es de tipo ObjectId
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    [BsonElement("Name")] // Esto indica el nombre del campo en la colección
    public string BookName { get; set; } = null!;

    public decimal Price { get; set; }

    public string Category { get; set; } = null!;

    public string Author { get; set; } = null!;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Esto indica que si el campo es null no se incluirá en la respuesta
    [BsonIgnoreIfNull] // Esto indica que si el campo es null no se incluirá en la colección
    public string? Image { get; set; } = null!;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [BsonIgnoreIfNull] 
    [JsonPropertyName("createdAt")] 
    public DateTime? CreatedAt { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [BsonIgnoreIfNull] 
    [JsonPropertyName("updatedAt")] 
    public DateTime? UpdatedAt { get; set; }
}