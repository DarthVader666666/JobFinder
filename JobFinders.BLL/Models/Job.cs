namespace JobFinders.BLL.Models
{
    public class Job
    {
        public int Index { get; set; }
        public string? Source { get; set; }
        public string? Link { get; set; }
        public string? Title { get; set; }
        public string? Location { get; set; }
        public string? Company { get; set; }
        public string? Experience { get; set; }
        public string? TimePosted { get; set; }
        public bool Saved { get; set; } = false;
        public Salary? Salary { get; set; }
        public Salary? OriginalSalary { get; set; }
        public Logo? Logo { get; set; }
    }

    public class Salary
    { 
        public int? Min { get; set; }
        public int? Max { get; set; }
        public string? Currency { get; set; } = "";
    }

    public class Logo
    { 
        public string? Source { get; set; } = "";
        public string? Url { get; set; } = "";
    }
}
