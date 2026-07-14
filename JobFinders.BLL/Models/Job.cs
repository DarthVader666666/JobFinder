namespace JobFinders.Bll.Models
{
    public class Job
    {
        public string? Link { get; set; }
        public string? Title { get; set; }
        public string? Location { get; set; }
        public string? Company { get; set; }
        public string? Experience { get; set; }
        public string? TimePosted { get; set; }
        public Salary? Salary { get; set; }
        public Logo? Logo { get; set;  }
    }

    public class Salary
    { 
        public int? Max { get; set; }
        public int? Min { get; set; }
        public string? Currency { get; set; } = "";
    }

    public class Logo
    { 
        public string? Source { get; set; } = "";
        public string? Url { get; set; } = "";
    }
}
