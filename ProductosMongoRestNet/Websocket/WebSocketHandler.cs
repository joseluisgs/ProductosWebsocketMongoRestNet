using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using ProductosMongoRestNet.Models;

namespace ProductosMongoRestNet.Websocket;

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
        // Escribimos e ignoramos los valores nulos para evitar errores de serialización e idnetamos
        var jsonSettings = new JsonSerializerSettings { 
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };
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