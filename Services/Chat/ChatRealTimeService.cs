public class ChatRealTimeService
{
    private readonly WebSocketChatClient _ws;
    private readonly IHttpContextAccessor _ctx;

    public ChatRealTimeService(WebSocketChatClient ws, IHttpContextAccessor ctx)
    {
        _ws = ws;
        _ctx = ctx;
    }

    public async Task<IAsyncEnumerable<string>> ConnectConversationAsync(string conversationId)
    {
        var token = _ctx.HttpContext!.Session.GetString("access_token");

        var wsUrl = $"ws://localhost:8081/ws/chat?access_token={token}";

        await _ws.ConnectAsync(wsUrl);
        await _ws.SendHandshakeAsync();
        await _ws.JoinConversationAsync(conversationId);

        return _ws.ReceiveAsync();
    }
}
