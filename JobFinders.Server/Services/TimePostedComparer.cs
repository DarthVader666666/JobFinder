using System.Globalization;

using JobFinders.BLL.Models;

namespace JobFinders.Server.Services
{
    public class TimePostedComparer : IComparer<Job>
    {
        private readonly DateTime now = DateTime.UtcNow.AddHours(3);
        private readonly string[] years = ["год", "года", "лет"];
        private readonly string[] months = ["месяц", "месяцев", "месяца", "мес."];
        private readonly string[] weeks = ["недели", "неделю", "недель", "неделя", "нед."];
        private readonly string[] days = ["день", "дней", "дня", "дн."];
        private readonly string[] hours = ["час", "часов", "часа"];
        private readonly string[] yesterday = ["вчера"];
        private readonly string[] today = ["сегодня"];
        private readonly string[] exactDate = ["декабр", "январ", "феврал", "март", "апрел", "ма", "июн", "июл", "август", "сентябр", "октябр", "ноябр"];

        public int Compare(Job? x, Job? y)
        {
            var firstTimeValue = string.IsNullOrEmpty(x?.TimePosted) ? x?.Index : GetTimeValue(x.TimePosted);
            var secondTimeValue = string.IsNullOrEmpty(y?.TimePosted) ? y?.Index : GetTimeValue(y.TimePosted);
            var compareResult = firstTimeValue?.CompareTo(secondTimeValue) ?? 0;


            if (compareResult == 0)
            {
                var first = firstTimeValue + x.Index;
                var second = secondTimeValue + y.Index;

                return first?.CompareTo(second) ?? 0;
            }

            return compareResult;
        }

        private int GetTimeValue(string time)
        {
            try
            {
                var result = time switch
                {
                    var t when Contains(t, hours) || Contains(t, today) => 0,
                    var t when Contains(t, yesterday) => 1,
                    var t when Contains(t, days) => GetDaysSpan(t, 1),
                    var t when Contains(t, weeks) => GetDaysSpan(t, 7),
                    var t when Contains(t, months) => GetDaysSpan(t, 30),
                    var t when Contains(t, years) => GetDaysSpan(t, 365),
                    var t when Contains(t, exactDate) => CompareDate(time),
                    _ => 366
                };

                return result;
            }
            catch
            {
                return 366;
            }
        }

        private static bool Contains(string? time, string[] array)
        {
            return !string.IsNullOrEmpty(time) && array.Any(item => time.Contains(item, StringComparison.InvariantCultureIgnoreCase));
        }

        private static int GetDaysSpan(string time, int multiplier)
        {
            var timeArray = time.Split(' ');

            if (timeArray.Length > 1)
            {
                var result = timeArray.FirstOrDefault(t => int.TryParse(t, out _));
                return result is null ? multiplier : int.Parse(result) * multiplier;
            }

            return multiplier;
        }

        private int CompareDate(string time)
        {
            var daysSpan = (now - DateTime.Parse(time ?? "", new CultureInfo("ru-RU"))).Days;
            return daysSpan;
        }
    }
}
