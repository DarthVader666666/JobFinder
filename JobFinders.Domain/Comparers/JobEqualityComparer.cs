using JobFinders.Domain.Models;

namespace JobFinders.Domain.Comparers
{
    public class JobEqualityComparer : IEqualityComparer<Job?>
    {
        private static readonly StringComparer _stringComparer = StringComparer.OrdinalIgnoreCase;

        public bool Equals(Job? x, Job? y)
        {
            return AreSalariesEqual(x?.Salary, y?.Salary) &&
                   AreLocationsEqual(x?.Location, y?.Location) &&
                   AreTitlesEqual(x?.Title, y?.Title) &&
                   AreCompaniesEqual(x?.Company, y?.Company);
        }

        public int GetHashCode(Job obj)
        {
            var hash = new HashCode();
            hash.Add(GetNormalizedSalaryHash(obj.Salary));
            hash.Add(_stringComparer.GetHashCode(obj.Location ?? string.Empty));
            hash.Add(_stringComparer.GetHashCode(obj.Title ?? string.Empty));
            hash.Add(GetNormalizedCompanyHash(obj.Company));

            return hash.ToHashCode();
        }

        private bool AreSalariesEqual(Salary? s1, Salary? s2)
        {
            return s1?.Min == s2?.Min &&
                   s1?.Max == s2?.Max &&
                   _stringComparer.Equals(s1?.Currency ?? string.Empty, s2?.Currency ?? string.Empty);
        }

        private bool AreLocationsEqual(string? loc1, string? loc2)
        {
            return _stringComparer.Equals(loc1?.Trim(), loc2?.Trim());
        }

        private bool AreTitlesEqual(string? title1, string? title2)
        {
            return _stringComparer.Equals(title1?.Trim(), title2?.Trim());
        }

        private bool AreCompaniesEqual(string? company1, string? company2)
        {
            var norm1 = NormalizeCompanyName(company1);
            var norm2 = NormalizeCompanyName(company2);

            return _stringComparer.Equals(norm1, norm2);
        }

        private string NormalizeCompanyName(string? company)
        {
            if (string.IsNullOrEmpty(company))
            {
                return string.Empty;
            }

            var normalized = company;
            var prefixes = new[] { "ООО", "ЗАО", "ОАО", "АО", "ИП", "ТОО", "ОДО" };

            foreach (var prefix in prefixes)
            {
                if (normalized.StartsWith(prefix))
                {
                    normalized = normalized.Replace(prefix , "");
                    break;
                }
            }

            normalized = normalized.Trim();
            normalized = normalized.Replace("\"", "").ToUpper();

            return normalized;
        }

        private int GetNormalizedSalaryHash(Salary? salary)
        {
            var hash = new HashCode();
            hash.Add(salary?.Min ?? 0);
            hash.Add(salary?.Max ?? 0);
            hash.Add(_stringComparer.GetHashCode(salary?.Currency ?? string.Empty));
            return hash.ToHashCode();
        }

        private int GetNormalizedCompanyHash(string? company)
        {
            return _stringComparer.GetHashCode(NormalizeCompanyName(company ?? ""));
        }
    }
}
