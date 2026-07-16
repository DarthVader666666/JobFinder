namespace JobFinders.BLL.Models
{
    public class JobsFilter
    {
        public bool ExactTitle { get; set; } = false;
        public bool SalaryDefined { get; set; } = false;
    }
}
