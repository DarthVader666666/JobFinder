using System.ComponentModel.DataAnnotations;

using JobFinders.BLL.Models;

namespace JobFinders.Server.Models
{
    public class JobsRequest
    {
        public string[]? Sources { get; set; }
        [Required]
        public string? Speciality { get; set; }
        public string? Location { get; set; }
        public Salary? Salary { get; set; }
    }
}
