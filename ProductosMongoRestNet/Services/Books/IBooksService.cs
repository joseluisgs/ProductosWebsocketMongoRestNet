using ProductosMongoRestNet.Models;

namespace ProductosMongoRestNet.Services;

public interface IBooksService
{
    public Task<List<Book>> GetAllAsync();
    public Task<Book?> GetByIdAsync(string id);
    public Task<Book> CreateAsync(Book book);
    public Task<Book?> UpdateAsync(string id, Book book);
    public Task<Book?> DeleteAsync(string id);
}