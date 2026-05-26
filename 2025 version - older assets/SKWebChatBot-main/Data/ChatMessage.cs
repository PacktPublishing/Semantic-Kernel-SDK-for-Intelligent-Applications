namespace SKWebChatBot.Data;

public class ChatMessage
    {
        public int Id { get; set; }
        public string SessionId { get; set; } // Unique per user session
        public string Message { get; set; }
        public string Sender { get; set; } // "User" or "Bot"
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }