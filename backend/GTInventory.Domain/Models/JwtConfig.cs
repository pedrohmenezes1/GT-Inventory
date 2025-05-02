namespace GTInventory.Domain.Models
{
    public class JwtConfig
    {
        public string? Secret { get; set; }
        public int ExpirationHours { get; set; }
    }
}