namespace ChatByWeb.Models
{
    public class DailyStats
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public int TotalMessages { get; set; }
        public int TotalFiles { get; set; }
        public long TotalStorageBytes { get; set; }
        public int NewConversations { get; set; }
    }
}
