# ProductosWebsocketMongoRestNet

Ejemplo de una API REST básica en .NET Core 8 con MongoDB y Websocket

![image](./image/image.webp)

- [ProductosWebsocketMongoRestNet](#productoswebsocketmongorestnet)
  - [Descripción](#descripción)
  - [Cómo hacerlo](#cómo-hacerlo)
  - [Endpoints](#endpoints)
  - [Librerías usadas](#librerías-usadas)


## Descripción

Este proyecto es un ejemplo de una API REST básica en .NET Core 8 con MongoDB con Websocket para crear un sistema de notificaciones.

Es una ampliación de este [proyecto](https://github.com/joseluisgs/ProductosStorageMongoRestNet)

Cuidado con las configuraciones y la inyección de los servicios

Mongo esta en Mongo Atlas, por lo que la cadena de conexión es un poco diferente.

## Cómo hacerlo
Lo primero que debemos hacer es un handler para el websocket, en este caso, lo he llamado `WebSocketHandler.cs` y es el encargado de gestionar las conexiones y las notificaciones.

```csharp
public class WebSocketHandler
{
    private readonly ILogger _logger;
    private readonly List<WebSocket> _sockets = new();

    public WebSocketHandler(ILogger<WebSocketHandler> logger)
    {
        _logger = logger;
    }

    // Este método se encarga de manejar las conexiones WebSocket entrantes
    public async Task HandleAsync(WebSocket webSocket)
    {
        _logger.LogInformation("WebSocket connected from {0}", webSocket);
        _sockets.Add(webSocket);

        var buffer = new byte[1024 * 4]; // Buffer para leer los datos del WebSocket
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        // Mientras la conexión esté abierta, leemos los datos del WebSocket
        while (!result.CloseStatus.HasValue)
            // Convertimos los datos recibidos a texto
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        // Cerramos la conexión WebSocket y la eliminamos de la lista
        _sockets.Remove(webSocket);
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }

    // Este método se encarga de enviar un mensaje a todos los clientes conectados
    public async Task NotifyAllAsync<T>(Notification<T> notification)
    {
        // Escribimos e ignoramos los valores nulos para evitar errores de serialización
        var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        var json = JsonConvert.SerializeObject(notification, jsonSettings);
        _logger.LogInformation($"Notifying all clients: {json}");
        var buffer = Encoding.UTF8.GetBytes(json);
        // Enviamos el mensaje a todos los clientes conectados
        var tasks = _sockets.Select(socket =>
                socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
                    CancellationToken.None))
            .ToArray();
        await Task.WhenAll(tasks); // Esperamos a que todos los envíos se completen
    }
}
```

Creamos como servicio en el `Program.cs`

```csharp
 // WebSocketHandler
    myBuilder.Services.AddSingleton<WebSocketHandler>();
```	

El siguiente paso es crear un controlador para los ws se pueda conectar y desconectar, en este caso, lo he llamado `WebSocketController.cs`

```csharp
[Route("api/[controller]")]
[ApiController]
public class WebSocketController : ControllerBase
{
    private readonly WebSocketHandler _webSocketHandler;

    public WebSocketController(WebSocketHandler webSocketHandler)
    {
        _webSocketHandler = webSocketHandler;
    }

    // WebSocket endpoint
    [HttpGet("/ws")]
    public async Task Get()
    {
        // Handle WebSocket connections
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            // Accept the WebSocket connection and handle messages/events within the WebSocketHandler class.
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await _webSocketHandler.HandleAsync(webSocket);
        }
        else
        {
            // Bad request response
            HttpContext.Response.StatusCode = 400;
        }
    }
}
```	

Luego debemos activar el middleware de Websocket en el `Program.cs`

```csharp
/ Usamos HTTPS redirection
app.UseHttpsRedirection();

// Habilitamos el middleware de WebSockets
app.UseWebSockets();
```

Ya lo podemos usar en el controlador, inyectando el Handler de los libros, en este caso, lo he llamado `BooksController.cs`

```csharp

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
```

Te puedes conectar en Postam con: `ws://localhost:5000/ws`



## Endpoints
- Books: contiene el CRUD de los libros (GET, POST, PUT, DELETE)
- ws: contiene el websocket para las notificaciones

## Librerías usadas
- MongoDB.Driver