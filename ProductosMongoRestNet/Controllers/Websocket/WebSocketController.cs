using Microsoft.AspNetCore.Mvc;
using ProductosMongoRestNet.Websocket;

namespace ProductosMongoRestNet.Config.Websocket;

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