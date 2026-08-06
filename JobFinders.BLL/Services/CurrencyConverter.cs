using JobFinders.BLL.Interfaces;
using JobFinders.BLL.Models;

namespace JobFinders.BLL.Services
{
    public class CurrencyConverter: ICurrencyConverter
    {
        private readonly Dictionary<string, string> currenciesApi = new() { ["$"] = "USD", ["€"] = "EUR", ["₽"] = "RUB", ["BYN"] = "BYN", ["₸"] = "KZT", ["₾"] = "GEL", ["₼"] = "AZN", ["so'm"] = "UZS", };

        public Salary? Convert(Salary? salary, JobsFilter? filter)
        {
            if (filter?.Salary?.Currency == "Нет")
            { 
                return salary;
            }

            if (salary is null || salary.Currency == filter?.Salary?.Currency)
            {
                return salary;
            }

            var jobCurrencyData = filter?.CurrencyRates?.FirstOrDefault(rate => rate.Abbreviation == currenciesApi[salary?.Currency ?? ""]);
            var apiCurrencyData = filter?.CurrencyRates?.FirstOrDefault(rate => rate.Abbreviation == currenciesApi[filter?.Salary?.Currency ?? ""]);

            var jobRate = jobCurrencyData?.Rate / jobCurrencyData?.Scale;
            var convertRate = apiCurrencyData?.Rate / apiCurrencyData?.Scale;

            var rate = jobRate / convertRate;

            salary?.Min = (int?)Math.Round((float)(salary?.Min * rate ?? 0));
            salary?.Max = (int?)Math.Round((float)(salary?.Max * rate ?? 0));
            salary?.Currency = filter?.Salary?.Currency;

            return salary;
        }
    }
}
