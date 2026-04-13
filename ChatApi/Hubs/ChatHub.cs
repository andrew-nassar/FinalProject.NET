using ChatApi.Data;
using ChatApi.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ChatApi.Hubs
{
    public class ChatHub : Hub
    {
        // userId => connectionId (one connection per user here)
        private static ConcurrentDictionary<string, string> _connections = new();

        private readonly AppDbContext _db;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(AppDbContext db, ILogger<ChatHub> logger)
        {
            _db = db;
            _logger = logger;
        }

        // Called by client right after connection to register its userId
        public async Task Register(string userId)
        {
            _connections.AddOrUpdate(userId, Context.ConnectionId, (k, v) => Context.ConnectionId);
            _logger.LogInformation("User {userId} registered with connection {cid}", userId, Context.ConnectionId);
            await Clients.Caller.SendAsync("Registered", userId);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // remove mapping if exists
            var item = _connections.FirstOrDefault(kvp => kvp.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(item.Key))
            {
                _connections.TryRemove(item.Key, out _);
            }
            return base.OnDisconnectedAsync(exception);
        }

        // Send message: saves to DB and forwards to receiver if online
        public async Task SendMessage(string senderId, string receiverId, string content)
        {
            var msg = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _db.Messages.Add(msg);
            await _db.SaveChangesAsync();

            // Prepare payload
            var payload = new
            {
                id = msg.Id,
                senderId = msg.SenderId,
                receiverId = msg.ReceiverId,
                content = msg.Content,
                createdAt = msg.CreatedAt,
                isRead = msg.IsRead
            };

            // If receiver is connected, send directly
            if (_connections.TryGetValue(receiverId, out var receiverConnId))
            {
                await Clients.Client(receiverConnId).SendAsync("ReceiveMessage", payload);
            }

            // Also notify sender with the saved message (so sender has id/timestamp)
            await Clients.Caller.SendAsync("MessageSent", payload);
        }
    }
}
