using System.ComponentModel.DataAnnotations;

namespace JobFinders.Data.Entities
{
    public class JobFinder
    {
        [Key]
        public int JobFinderId { get; set; }
        public string? Name { get; set; }
        public string? LinkTemplate { get; set; }
        public string? CssClass { get; set; }
        public string? HrefPrefix { get; set; }
        public string? BaseUrl { get; set; }
        public string? LocationTransliteration { get; set; }
        public string? NodeTag { get; set; } = "div";
        public bool AddBaseUrlToHrefPrefix { get; set; } = false;
        public bool ZeroPagination { get; set; } = false;
        public bool MandatoryLocation { get; set; } = false;
    }
}
