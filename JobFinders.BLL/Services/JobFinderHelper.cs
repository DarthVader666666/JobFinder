using System.Text.Json;

using JobFinders.Bll.Models;

namespace JobFinders.Bll.Services
{
    public static class JobFinderHelper
    {
        public static readonly string LocationPlaceholder = "#area#";
        public static readonly string SpecialityPlaceholder = "#speciality#";
        public static readonly string PagePlaceholder = "#page#";

        public static List<JobFinderSetting>? JobFinderSettings;

        public static void InitializeSettings(string? jsonConfig)
        {
            if(jsonConfig is null)
            {
                throw new ArgumentNullException(nameof(jsonConfig));
            }

            JobFinderSettings = JsonSerializer.Deserialize<List<JobFinderSetting>>(jsonConfig);
        }
    }
}
