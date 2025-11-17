namespace ChatByWeb.Models
{
    public class MessageModel
    {
        public string Id { get; set; }
        public string ConversationId { get; set; } // Giữ khóa ngoại
        public string SenderId { get; set; }
        public string Content { get; set; }

        public DateTimeOffset SentAt { get; set; }
    }
}
