using JobFinders.BLL.Models;

namespace JobFinders.BLL.Interfaces
{
    public interface ICurrencyConverter
    {
        public Salary? Convert(Salary? salary, JobsFilter? filter);
    }
}
