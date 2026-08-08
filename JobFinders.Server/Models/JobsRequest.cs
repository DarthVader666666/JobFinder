using System.ComponentModel.DataAnnotations;

using JobFinders.BLL.Models;

namespace JobFinders.Server.Models
{
    public class JobsRequest
    {
        [Required]
        public string? Speciality { get; set; }
        public string? Location { get; set; }
        public string[]? Sources { get; set; }
        public Filter? Filter { get; set; }        
    }

    public class Filter {
        public Salary? Salary { get; set; }
    }
}
