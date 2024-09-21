using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using ProductosMongoRestNet.Database;
using ProductosMongoRestNet.Models;

namespace ProductosMongoRestNet.Services;

public class BooksService : IBooksService
{
    private const string CacheKeyPrefix = "Book_"; //Para evitar colisiones en la caché de memoria con otros elementos
    private readonly IMongoCollection<Book> _booksCollection; // o Modelo O Documento de MongoDB
    private readonly ILogger _logger;
    private readonly IMemoryCache _memoryCache;

    public BooksService(IOptions<BookStoreMongoConfig> bookStoreDatabaseSettings, ILogger<BooksService> logger,
        IMemoryCache memoryCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
        var mongoClient = new MongoClient(bookStoreDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);
        _booksCollection =
            mongoDatabase.GetCollection<Book>(bookStoreDatabaseSettings.Value.BooksCollectionName);
    }

    public async Task<List<Book>> GetAllAsync()
    {
        _logger.LogInformation("Getting all books from database");
        return await _booksCollection.Find(_ => true).ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(string id)
    {
        _logger.LogInformation($"Getting book with id: {id}");
        var cacheKey = CacheKeyPrefix + id;

        // Primero intentamos obtener el libro de la caché
        if (_memoryCache.TryGetValue(cacheKey, out Book? cachedBook))
        {
            _logger.LogInformation("Getting book from cache");
            return cachedBook;
        }

        // Si no está en la caché, lo obtenemos de la base de datos
        _logger.LogInformation("Getting book from database");
        var book = await _booksCollection.Find(book => book.Id == id).FirstOrDefaultAsync();

        // Si el libro está en la base de datos, lo guardamos en la caché
        if (book != null)
        {
            _logger.LogInformation("Book not found in cache, caching it");
            _memoryCache.Set(cacheKey, book,
                TimeSpan.FromMinutes(30)); // Ajusta el tiempo de caché según tus necesidades
            _logger.LogInformation("Caching the book");
        }

        // Devolvemos el libro
        return book;
    }

    public async Task<Book> CreateAsync(Book book)
    {
        _logger.LogInformation("Creating a new book");

        // Cambiamos el Id del libro por un nuevo ObjectId (si no lo tiene), lo generaría MongoDB
        book.Id = ObjectId.GenerateNewId().ToString();
        var timeStamp = DateTime.Now;
        book.CreatedAt = timeStamp;
        book.UpdatedAt = timeStamp;

        // Inserta el documento en la base de datos
        await _booksCollection.InsertOneAsync(book); // No lo guarda por eso generamos el Id

        _logger.LogInformation($"Book created with id: {book.Id}");

        // Convierte el documento a la entidad Book
        return book;
    }

    public async Task<Book?> UpdateAsync(string id, Book book)
    {
        _logger.LogInformation($"Updating book with id: {id}");

        // Le ponemos el Id al libro, por si viene sin él
        book.Id = id;
        book.UpdatedAt = DateTime.Now;

        // Realiza el reemplazo y devuelve el documento actualizado, devolvemos el documento actualizado (After)
        var updatedBook = await _booksCollection.FindOneAndReplaceAsync<Book>(
            document => document.Id == id,
            book,
            new FindOneAndReplaceOptions<Book> { ReturnDocument = ReturnDocument.After }
        );

        // Gestionamos la caché
        var cacheKey = CacheKeyPrefix + id;

        // Eliminamos el libro de la caché si se ha actualizado
        if (updatedBook != null)
        {
            _memoryCache.Remove(cacheKey); // Invalida la clave de la caché
            _logger.LogInformation($"Removed cached book with id: {id}");
        }

        _logger.LogInformation($"Book updated with id: {id}");

        return updatedBook;
    }

    public async Task<Book?> DeleteAsync(string id)
    {
        _logger.LogInformation($"Deleting book with id: {id}");

        // Elimina el documento de la base de datos y devuelve el documento eliminado
        var deletedBook = await _booksCollection.FindOneAndDeleteAsync(bookDocument => bookDocument.Id == id);

        // Genera la clave de caché
        var cacheKey = CacheKeyPrefix + id;

        // Elimina el libro de la caché si existe
        if (deletedBook != null)
        {
            _memoryCache.Remove(cacheKey);
            _logger.LogInformation($"Removed cached book with id: {id}");
        }

        _logger.LogInformation($"Book deleted with id: {id}");

        return deletedBook;
    }
}