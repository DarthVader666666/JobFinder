namespace JobFinders.BLL.Models
{
    public class JobsFilter
    {
        public string? Speciality { get; set; } = "";
        public string? Location { get; set; } = "minsk";
        public bool ExactTitle { get; set; } = false;
        public bool SalaryDefined { get; set; } = false;
        public bool OrderBySalary { get; set; } = false;
        public Salary? Salary { get; set; }
        public ICollection<CurrencyData>? CurrencyRates { get; set; } = [];
    }
}
