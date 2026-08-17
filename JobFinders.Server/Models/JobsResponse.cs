using JobFinders.BLL.Models;

namespace JobFinders.Server.Models
{
    public class JobsResponse
    {
        public Job[][]? JobGroups { get; set; }
        public bool HasMoreJobs { get; set; } = false;
    }
}