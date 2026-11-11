namespace JobFinders.Domain.Entities
{
    public class RefreshToken
    {
        public int TokenId { get; set; }
        public string? Token { get; set; }
        public int UserId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsRevoked { get; set; } = false;
        public virtual User? User { get; set; }
    }
}
