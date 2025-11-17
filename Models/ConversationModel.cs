namespace ChatByWeb.Models
{
    public class ConversationModel
    {
        public string Id { get; set; }
        public bool IsDirect { get; set; }
        public string Title { get; set; }
        public string DirectKey { get; set; }
        public long Version { get; set; }

        // API sẽ trả về một mảng [ "id1", "id2" ]
        public List<string> Members { get; set; }
    }
}
