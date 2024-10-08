﻿using Microsoft.AspNetCore.Mvc;
using ProductosMongoRestNet.Models;
using ProductosMongoRestNet.Services;
using ProductosMongoRestNet.Services.Storage;
using ProductosMongoRestNet.Websocket;

namespace ProductosMongoRestNet.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Route("api/books")]
public class BooksController : ControllerBase
{
    private const string _route = "api/storage";
    private readonly IBooksService _booksService;
    private readonly IFileStorageService _storageService;
    private readonly WebSocketHandler _webSocketHandler; // Añadimos el WebSocketHandler

    public BooksController(IBooksService booksService, IFileStorageService storageService,
        WebSocketHandler webSocketHandler)
    {
        _booksService = booksService;
        _storageService = storageService;
        _webSocketHandler = webSocketHandler; // Inyectamos el WebSocketHandler
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

        // Enviamos la notificación a todos los clientes conectados
        var notification = new Notification<Book>
        {
            Data = savedBook,
            Type = typeof(Notification<Book>.NotificationType).GetEnumName(Notification<Book>.NotificationType
                .Create),
            CreatedAt = DateTime.Now
        };
        await _webSocketHandler.NotifyAllAsync(notification);

        // Devolvemos la respuesta con el libro creado
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, savedBook);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<ActionResult> Update(
        string id,
        [FromBody] Book book)
    {
        var updatedBook = await _booksService.UpdateAsync(id, book);

        if (updatedBook is null) return NotFound("Book not found with the provided id: " + id);

        // Enviamos la notificación a todos los clientes conectados
        var notification = new Notification<Book>
        {
            Data = updatedBook,
            Type = typeof(Notification<Book>.NotificationType).GetEnumName(Notification<Book>.NotificationType
                .Update),
            CreatedAt = DateTime.Now
        };
        await _webSocketHandler.NotifyAllAsync(notification);

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
            if (!string.IsNullOrEmpty(deletedBook.Image)) await _storageService.DeleteFileAsync(deletedBook.Image);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        // Eliminamos la notificación a todos los clientes conectados
        var notification = new Notification<Book>
        {
            Data = deletedBook,
            Type = typeof(Notification<Book>.NotificationType).GetEnumName(Notification<Book>.NotificationType
                .Delete),
            CreatedAt = DateTime.Now
        };
        await _webSocketHandler.NotifyAllAsync(notification);

        // Devolvemos la respuesta con el libro eliminado
        return NoContent();
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

            // Avismamos a los clientes conectados
            var notification = new Notification<Book>
            {
                Data = book,
                Type = typeof(Notification<Book>.NotificationType).GetEnumName(Notification<Book>.NotificationType
                    .Update),
                CreatedAt = DateTime.Now
            };
            await _webSocketHandler.NotifyAllAsync(notification);

            // Devolvemos el libro actualizado con la nueva URL de la imagen
            return Ok(await _booksService.UpdateAsync(id, book));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}