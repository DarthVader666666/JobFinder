namespace JobFinders.Domain.Models
{
    public class PageCounterQuery(string? source, string? speciality, string? location, int currentPage = 0, bool hasNextPage = false)
    {
        public string? Source { get; set; } = source;
        public string? Speciality { get; set; } = speciality;
        public string? Location { get; set; } = location;
        public int? CurrentPage { get; set; } = currentPage;
        public bool HasNextPage { get; set; } = hasNextPage;
    }
}
