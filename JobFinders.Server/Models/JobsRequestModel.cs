namespace JobFinders.Server.Models
{
    public class JobsRequestModel
    {
        public string? Url { get; set; }
        public string? Speciality { get; set; }
        public string? Area { get; set; }
        public string[]? Sources { get; set; }
    }
}
