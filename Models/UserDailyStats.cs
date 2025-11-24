namespace ChatByWeb.Models
{
    public class UserDailyStats
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
        public int MessagesSent { get; set; }
        public int FilesUploaded { get; set; }
        public long StorageUsedBytes { get; set; }
    }
}
