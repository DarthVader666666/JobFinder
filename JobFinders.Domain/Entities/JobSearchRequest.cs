namespace JobFinders.Domain.Entities
{
    public class JobSearchRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Speciality { get; set; }
        public string? Location { get; set; }
        public string? Sources { get; set; }
        public DateTime TimeSent { get; set; }
        public virtual User? User { get; set; }
    }
}
