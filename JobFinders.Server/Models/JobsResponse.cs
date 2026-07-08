using JobFinders.Bll.Models;

namespace JobFinders.Server.Models
{
    public class JobsResponse
    {
        public string? Source { get; set; }
        public string? Link { get; set; }
        public List<Job>? Jobs { get; set; }
    }
}
