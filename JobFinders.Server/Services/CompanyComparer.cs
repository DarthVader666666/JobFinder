using JobFinders.BLL.Models;

namespace JobFinders.Server.Services
{
    public class CompanyComparer: IEqualityComparer<Job?>
    {
        public bool Equals(Job? job1, Job? job2)
        {
            var company1Parts = job1?.Company?.Split([' ', '-']) ?? [];
            var company2Parts = job2?.Company?.Split([' ', '-']) ?? [];
            var salariesDefined = job1?.OriginalSalary?.Min is not null && job2?.OriginalSalary?.Min is not null &&
                job1?.OriginalSalary?.Max is not null && job2?.OriginalSalary?.Max is not null &&
                job1?.OriginalSalary?.Currency is not null && job2?.OriginalSalary?.Currency is not null;

            return job1?.Title == job2?.Title &&
                (!salariesDefined || job1?.OriginalSalary?.Currency == job2?.OriginalSalary?.Currency) && 
                (!salariesDefined || job1?.OriginalSalary?.Min == job2?.OriginalSalary?.Min) && 
                (!salariesDefined || job1?.OriginalSalary?.Max == job2?.OriginalSalary?.Max) &&
                (company1Parts.Any(c1p => job2?.Company?.Contains(c1p, StringComparison.InvariantCultureIgnoreCase) ?? false) ||
                company2Parts.Any(c2p => job1?.Company?.Contains(c2p, StringComparison.InvariantCultureIgnoreCase) ?? false));
        }

        public int GetHashCode(Job obj)
        {            
            return obj.Title?.GetHashCode() ?? 0;
        }
    }
}
