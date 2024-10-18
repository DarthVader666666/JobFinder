namespace JobFinder.Server.Models
{
    public class ResponseModel
    {
        public string? SourceName { get; set; }
        public string? SourceUrl { get; set; }
        public IEnumerable<JobModel>? Jobs { get; set; }
    }
}
