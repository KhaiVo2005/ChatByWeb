namespace ChatByWeb.Models
{
    public class KeycloakTokenResponse
    {
        public required string access_token { get; set; }
        public required string refresh_token { get; set; }
        public int expires_in { get; set; }
        public int refresh_expires_in { get; set; }
        public required string token_type { get; set; }
    }

}
