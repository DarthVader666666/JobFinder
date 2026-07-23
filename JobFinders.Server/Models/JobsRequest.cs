namespace JobFinders.Server.Models
{
    public class JobsRequest
    {
        public string? Speciality { get; set; }
        public string? Location { get; set; }
        public string[]? Sources { get; set; }
        public Filter? Filter { get; set; }    
    }

    public class Filter {
        public bool ExactTitle { get; set; } = false;
        public bool SalaryDefined { get; set; } = false;
        public bool OrderBySalary { get; set; } = false;
    }
}
