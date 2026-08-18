using System.Globalization;

namespace JobFinders.Server.Services
{
    public class TimePostedComparer : IComparer<(int index, string? time)>
    {
        private readonly string[] years = ["год", "года", "лет"];
        private readonly string[] months = ["месяц", "месяцев", "месяца", "мес."];
        private readonly string[] weeks = ["недели", "неделю", "недель", "неделя", "нед."];
        private readonly string[] days = ["день", "дней", "дня", "дн."];
        private readonly string[] hours = ["час", "часов", "часа"];
        private readonly string[] yesterday = ["вчера"];
        private readonly string[] today = ["сегодня"];

        public int Compare((int index, string? time) x, (int index, string? time) y)
        {
            var firstTimeValue = GetTimeValue(x.time);
            var secondTimeValue = GetTimeValue(y.time);
            var compareResult = firstTimeValue.CompareTo(secondTimeValue);


            if (compareResult == 0)
            {
                var first = firstTimeValue + x.index;
                var secnd = secondTimeValue + y.index;

                return first.CompareTo(secnd);
            }

            return compareResult;
        }

        private int GetTimeValue(string? time)
        {
            bool Contains(string[] array)
            {
                return !string.IsNullOrEmpty(time) && array.Any(item => time.Contains(item, StringComparison.InvariantCultureIgnoreCase));
            }

            int CompareDate()
            {
                if (DateTime.TryParse(time ?? "", new CultureInfo("ru-RU"), out DateTime datePosted))
                {
                    var today = DateTime.UtcNow.AddHours(3);
                    var days = (today - datePosted).Days;

                    var result = days switch
                    {
                        0 => 2,
                        1 => 3,
                        > 1 and < 7 => 4,
                        >= 7 and <= 30 => 5,
                        > 30 and < 365 => 6,
                        >= 365 => 7,
                        _ => 8
                    };

                    return result;
                }
                else
                {
                    return 5;
                }
            }

            var result = time switch
            {
                var t when string.IsNullOrEmpty(t) => 5,
                var t when Contains(hours) => 1,
                var t when Contains(today) => 2,
                var t when Contains(yesterday) => 3,
                var t when Contains(days) => 4,
                var t when Contains(weeks) => 5,
                var t when Contains(months) => 6,
                var t when Contains(years) => 7,
                _ => CompareDate(),
            };

            return result;
        }
    }
}
