namespace JobFinders.Domain.Models
{
    public class PageCounter(string source)
    {
        public string? Source { get; set; } = source;
        public int CurrentPage { get; set; } = 0;
        public bool? HasNextPage { get; set; }
    }
}
