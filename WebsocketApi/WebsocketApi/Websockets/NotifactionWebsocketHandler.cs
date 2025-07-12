using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace WebsocketApi.Websockets
{
    // Have a singleton instance of this class to manage WebSocket connections of a given type.
    public class NotifactionWebsocketHandler
    {
        private static readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new();

        private readonly RequestDelegate _next;

        public NotifactionWebsocketHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/ws/notifications")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    var id = Guid.NewGuid();
                    _sockets.TryAdd(id, socket);

                    // Keep the connection open
                    var buffer = new byte[1024 * 4];
                    while (socket.State == WebSocketState.Open)
                    {
                        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            break;
                        }
                    }

                    _sockets.TryRemove(id, out _);
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocket handler", CancellationToken.None);
                    return;
                }
                else
                {
                    context.Response.StatusCode = 400;
                    return;
                }
            }

            await _next(context);
        }
        public async Task ReceiveMessagesAsync(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;
            do
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Handle text messages here
                    string message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received: {message}");
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
            } while (!result.CloseStatus.HasValue);
        }

        public static async Task BroadcastAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var tasks = _sockets.Values
                .Where(s => s.State == WebSocketState.Open)
                .Select(socket => socket.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None));

            await Task.WhenAll(tasks);
        }
    }

}
