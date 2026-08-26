namespace JobFinders.Domain.Models
{
    public record JobFinderSetting
    {
        public string? Source { get; set; }
        public string? LinkTemplate { get; set; }
        public string? HrefPrefix { get; set; }
        public string? BaseUrl { get; set; }
        public string? LocationTransliteration { get; set; } = "Cyrillic";
        public bool AddBaseUrlToHrefPrefix { get; set; } = false;
        public bool ZeroBasedPagination { get; set; } = false;
        public bool MandatoryLocation { get; set; } = false;
        public bool ConvertLocation { get; set; } = false;
        public Dictionary<string, string>? LocationDictionary { get; set; }
        public JobFinderTag? VacancyTag { get; set; }
        public JobFinderTag? NavigationTag { get; set; }
        public JobTag? Salary { get; set; }
        public JobTag? Location { get; set; }
        public JobTag? Company { get; set; }
        public JobTag? Experience { get; set; }
        public JobTag? TimePosted { get; set; }
    }

    public class JobTag
    {
        public string? Attribute { get; set; } = "class";
        public string? Value { get; set; }
    }

    public class JobFinderTag
    {
        public string? Tag { get; set; } = "div";
        public JobTag? HtmlAttribute { get; set; }
    }
}
