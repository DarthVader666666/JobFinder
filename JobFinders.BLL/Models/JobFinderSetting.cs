namespace JobFinders.Bll.Models
{
    public record JobFinderSetting
    {
        public string? Source { get; set; }
        public string? LinkTemplate { get; set; }
        public string? TagCssClass { get; set; }
        public string? SalaryCssClass { get; set; }
        public string? HrefPrefix { get; set; }
        public string? BaseUrl { get; set; }
        public string? LocationTransliteration { get; set; }
        public string? NodeTag { get; set; } = "div";
        public bool AddBaseUrlToHrefPrefix { get; set; } = false;
        public bool ZeroBasedPagination { get; set; } = false;
        public bool MandatoryLocation { get; set; } = false;
        public CssAttribute? Location { get; set; }
        public CssAttribute? Company { get; set; }
        public CssAttribute? Experience { get; set; }
    }

    public class CssAttribute
    {
        public string? Attribute { get; set; } = "class";
        public string? Value { get; set; }
    }
}
