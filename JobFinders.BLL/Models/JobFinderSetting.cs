namespace JobFinders.Bll.Models
{
    public record JobFinderSetting
    {
        public string? Source { get; set; }
        public string? LinkTemplate { get; set; }
        public string? HrefPrefix { get; set; }
        public string? BaseUrl { get; set; }
        public string? LocationTransliteration { get; set; }
        public bool AddBaseUrlToHrefPrefix { get; set; } = false;
        public bool ZeroBasedPagination { get; set; } = false;
        public bool MandatoryLocation { get; set; } = false;
        public HtmlTag? VacancyTag { get; set; }
        public HtmlAttribute? Salary { get; set; }
        public HtmlAttribute? Location { get; set; }
        public HtmlAttribute? Company { get; set; }
        public HtmlAttribute? Experience { get; set; }
        public HtmlAttribute? TimePosted { get; set; }
    }

    public class HtmlAttribute
    {
        public string? Attribute { get; set; } = "class";
        public string? Value { get; set; }
    }

    public class HtmlTag
    {
        public string? Tag { get; set; } = "div";
        public HtmlAttribute? HtmlAttribute { get; set; }
    }
}
