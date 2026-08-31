namespace JobFinders.Domain.Entities
{
    public class ConfirmationCode
    {
        public int CodeId { get; set; }
        public int UserId { get; set; }
        public DateTime DateGenerated { get; set; }
        public string? Code { get; set; }
        public virtual User? User { get; set; }
    }
}
