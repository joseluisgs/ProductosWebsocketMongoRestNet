using Microsoft.AspNetCore.Mvc;
using ProductosMongoRestNet.Config.Storage;
using ProductosMongoRestNet.Exceptions.FileStorage;
using ProductosMongoRestNet.Services.Storage;

namespace ProductosMongoRestNet.Controllers.Storage;

[Route("api/[controller]")]
[ApiController]
public class StorageController : ControllerBase
{
    private const string _route = "api/storage";
    private readonly IFileStorageService _fileStorageService;

    public StorageController(IFileStorageService fileStorageService) 
    {
        _fileStorageService = fileStorageService;
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Not file in the request");

        try
        {
            var fileName = await _fileStorageService.SaveFileAsync(file);

            // Obtener la URL base
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var fileUrl = $"{baseUrl}/{_route}/{fileName}";

            return Ok(new { Url = fileUrl }); // Devolvemos la URL del fichero
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetFile(string fileName)
    {
        try
        {
            var fileStream = await _fileStorageService.GetFileAsync(fileName);
            // De esta manera iniciamos la descarga del fichero
            //return File(fileStream, "application/octet-stream", fileName); // Devolvemos el fichero como respuesta
            // Si lo dejamos previsualkiza y si no descarga
            var fileExtension = Path.GetExtension(fileName);
            var mimeType = MimeTypes.GetMimeType(fileExtension);
            return File(fileStream, mimeType, fileName); // Devolvemos el fichero como respuesta
        }
        catch (FileStorageException e)
        {
            return NotFound($"File not found with name: {fileName}"); // No se ha encontrado el fichero
        }
        catch (Exception e)
        {
            return StatusCode(500); // Error interno del servidor
        }
    }

    [HttpDelete("{fileName}")]
    public async Task<IActionResult> DeleteFile(string fileName)
    {
        try
        {
            var success = await _fileStorageService.DeleteFileAsync(fileName);
            if (!success)
                return NotFound($"File not found with name: {fileName}"); // No se ha encontrado el fichero
            return NoContent(); // Se ha eliminado correctamente
        }
        catch (Exception e)
        {
            return StatusCode(500); // Error interno del servidor
        }
    }
}