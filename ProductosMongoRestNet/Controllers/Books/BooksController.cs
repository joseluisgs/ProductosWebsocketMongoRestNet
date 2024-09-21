using Microsoft.AspNetCore.Mvc;
using ProductosMongoRestNet.Models;
using ProductosMongoRestNet.Services;
using ProductosMongoRestNet.Services.Storage;

namespace ProductosMongoRestNet.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Route("api/books")]
public class BooksController : ControllerBase
{
    private const string _route = "api/storage";
    private readonly IBooksService _booksService;
    private readonly IFileStorageService _storageService;

    public BooksController(IBooksService booksService, IFileStorageService storageService)
    {
        _booksService = booksService;
        _storageService = storageService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Book>>> GetAll()
    {
        var books = await _booksService.GetAllAsync();
        return Ok(books);
    }

    [HttpGet("{id:length(24)}")] // Para que el id tenga 24 caracteres (ObjectId)
    public async Task<ActionResult<Book>> GetById(string id)
    {
        var book = await _booksService.GetByIdAsync(id);

        if (book is null) return NotFound("Book not found with the provided id: " + id);

        return book;
    }

    [HttpPost]
    public async Task<ActionResult<Book>> Create(Book book)
    {
        var savedBook = await _booksService.CreateAsync(book);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, savedBook);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<ActionResult> Update(
        string id,
        [FromBody] Book book)
    {
        var updatedBook = await _booksService.UpdateAsync(id, book);

        if (updatedBook is null) return NotFound("Book not found with the provided id: " + id);

        return Ok(updatedBook);
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<ActionResult> Delete(string id)
    {
        var deletedBook = await _booksService.DeleteAsync(id);

        if (deletedBook is null) return NotFound("Book not found with the provided id: " + id);
        
        // Eliminamos la imagen
        try
        {
            if (!string.IsNullOrEmpty(deletedBook.Image))
            {
                await _storageService.DeleteFileAsync(deletedBook.Image);
            }
            return NoContent();
        } catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPatch("{id:length(24)}")]
    public async Task<ActionResult> UpdateImage(
        string id,
        [FromForm] IFormFile file)
    {
        
        // Comprobamos que el fichero no sea nulo
        if (file == null || file.Length == 0)
            return BadRequest("Not file in the request");
        
        // Obtenemos el libro
        var book = await _booksService.GetByIdAsync(id);
        
        // Si el libro no existe, devolvemos un error
        if (book is null) return NotFound("Book not found with the provided id: " + id);
        try
        {
            // Guardamos el fichero
            var fileName = await _storageService.SaveFileAsync(file);
            
            // Actualizamos la URL de la imagen
            // Aquí es cuando debemos decidir si la queremos el no,bre del fichero o la URL
            // Mira el controlador de Storage para ver cómo se hace, yo lo he hecho con el nombre del fichero
            // así siempre lo puedes contruir con la URL base, desde el cliente.
            // Obtener la URL base
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var fileUrl = $"{baseUrl}/{_route}/{fileName}";
            book.Image = fileUrl;
            //book.Image = fileName;
            return Ok(await _booksService.UpdateAsync(id, book));

        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}