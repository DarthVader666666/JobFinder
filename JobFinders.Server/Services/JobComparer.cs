using JobFinders.BLL.Models;

namespace JobFinders.Server.Services
{
    public class JobComparer : IEqualityComparer<Job?>
    {
        private static readonly StringComparer _stringComparer = StringComparer.OrdinalIgnoreCase;

        public bool Equals(Job? x, Job? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            return AreSalariesEqual(x.Salary, y.Salary) &&
                   AreLocationsEqual(x.Location, y.Location) &&
                   AreTitlesEqual(x.Title, y.Title) &&
                   AreCompaniesEqual(x.Company, y.Company);
        }

        public int GetHashCode(Job? obj)
        {
            if (obj is null) return 0;

            var hash = new HashCode();
            hash.Add(GetNormalizedSalaryHash(obj.Salary));
            hash.Add(_stringComparer.GetHashCode(obj.Location ?? string.Empty));
            hash.Add(_stringComparer.GetHashCode(obj.Title ?? string.Empty));
            hash.Add(GetNormalizedCompanyHash(obj.Company));

            return hash.ToHashCode();
        }

        private bool AreSalariesEqual(Salary? s1, Salary? s2)
        {
            if (ReferenceEquals(s1, s2)) return true;
            if (s1 is null || s2 is null) return false;

            return s1.Min == s2.Min &&
                   s1.Max == s2.Max &&
                   _stringComparer.Equals(s1.Currency ?? string.Empty, s2.Currency ?? string.Empty);
        }

        private bool AreLocationsEqual(string? loc1, string? loc2)
        {
            if (string.IsNullOrEmpty(loc1) && string.IsNullOrEmpty(loc2)) return true;
            if (string.IsNullOrEmpty(loc1) || string.IsNullOrEmpty(loc2)) return false;

            return _stringComparer.Equals(loc1.Trim(), loc2.Trim());
        }

        private bool AreTitlesEqual(string? title1, string? title2)
        {
            if (string.IsNullOrEmpty(title1) && string.IsNullOrEmpty(title2)) return true;
            if (string.IsNullOrEmpty(title1) || string.IsNullOrEmpty(title2)) return false;

            return _stringComparer.Equals(title1.Trim(), title2.Trim());
        }

        private bool AreCompaniesEqual(string? company1, string? company2)
        {
            if (string.IsNullOrEmpty(company1) && string.IsNullOrEmpty(company2)) return true;
            if (string.IsNullOrEmpty(company1) || string.IsNullOrEmpty(company2)) return false;

            var norm1 = NormalizeCompanyName(company1);
            var norm2 = NormalizeCompanyName(company2);

            return _stringComparer.Equals(norm1, norm2);
        }

        private string NormalizeCompanyName(string? company)
        {
            if (string.IsNullOrEmpty(company)) return string.Empty;

            var normalized = company.Trim();

            var prefixes = new[] { "ООО ", "ЗАО ", "ОАО ", "АО ", "ИП ", "ТОО " };
            foreach (var prefix in prefixes)
            {
                if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    normalized = normalized.Substring(prefix.Length).Trim();
                    break;
                }
            }

            var suffixes = new[] { " LLC", " Ltd", " Inc", " Corp", " GmbH", " AG" };
            foreach (var suffix in suffixes)
            {
                if (normalized.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    normalized = normalized.Substring(0, normalized.Length - suffix.Length).Trim();
                }
            }

            return normalized;
        }

        private int GetNormalizedSalaryHash(Salary? salary)
        {
            if (salary is null) return 0;

            var hash = new HashCode();
            hash.Add(salary.Min ?? 0);
            hash.Add(salary.Max ?? 0);
            hash.Add(_stringComparer.GetHashCode(salary.Currency ?? string.Empty));
            return hash.ToHashCode();
        }

        private int GetNormalizedCompanyHash(string? company)
        {
            if (string.IsNullOrEmpty(company)) return 0;
            return _stringComparer.GetHashCode(NormalizeCompanyName(company));
        }
    }
}
