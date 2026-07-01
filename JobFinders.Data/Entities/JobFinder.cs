namespace JobFinders.Data.Entities
{
    public class JobFinder
    {
        int JobFinderId { get; set; }
        string? Name { get; set; }
        string? CssClass { get; set; }
        bool AddBaseUrlToHref { get; set; }
        string? Href { get; set; }
        string? BaseUrl { get; set; }
        string? LinkTemplate { get; set; }
        string? LocationTransliteration { get; set; }
    }
}
