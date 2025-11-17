using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class WebSocketChatClient
{
    private readonly ClientWebSocket _socket = new();

    public async Task ConnectAsync(string url)
    {
        await _socket.ConnectAsync(new Uri(url), CancellationToken.None);
    }

    public async Task SendHandshakeAsync()
    {
        var handshake = "{\"protocol\":\"json\",\"version\":1}\x1e";
        var bytes = Encoding.UTF8.GetBytes(handshake);
        await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task JoinConversationAsync(string conversationId)
    {
        var obj = new
        {
            type = 1,
            target = "JoinConversation",
            arguments = new[] { conversationId }
        };

        var json = JsonSerializer.Serialize(obj) + "\x1e";
        var buffer = Encoding.UTF8.GetBytes(json);

        await _socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async IAsyncEnumerable<string> ReceiveAsync()
    {
        var buffer = new byte[4096];

        while (true)
        {
            var result = await _socket.ReceiveAsync(buffer, CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
                yield break;

            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            yield return message;
        }
    }
}
