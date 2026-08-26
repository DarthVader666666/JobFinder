using System.ComponentModel.DataAnnotations;

using JobFinders.Domain.Models;

namespace JobFinders.Api.Models
{
    public class JobsRequest
    {
        public string[]? Sources { get; set; }
        [Required]
        public string? Speciality { get; set; }
        public string? Location { get; set; }
        public Salary? Salary { get; set; }
        public bool MoreJobs { get; set; } = false;
    }
}
