namespace JobFinders.Server.Models
{
    public class JobsResponseModel
    {
        public string? SourceName { get; set; }
        public string? SourceUrl { get; set; }
        public IEnumerable<JobModel>? Jobs { get; set; }
    }
}
