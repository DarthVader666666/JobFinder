namespace JobFinders.Bll.Models
{
    public class Job
    {
        public string? Link { get; set; }
        public string? Title { get; set; }
        public Salary? Salary { get; set; }
    }

    public class Salary
    { 
        public int? Max { get; set; }
        public int? Min { get; set; }
        public string? Currency { get; set; }
    }
}
