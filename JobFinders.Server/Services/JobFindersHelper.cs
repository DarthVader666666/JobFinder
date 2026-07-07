using JobFinders.Data;
using JobFinders.Data.Entities;
using JobFinders.Server.Enums;

namespace JobFinders.Server.Services
{
    public static class JobFindersHelper
    {
        public const string LocationPlaceholder = "#area#";
        public const string SpecialityPlaceholder = "#speciality#";
        public const string PagePlaceholder = "#page#";

        public static void Seed(JobFindersDbContext dbContext)
        {
            if (!dbContext.JobFinders.Any())
            {
                dbContext.JobFinders.AddRange(
                    new JobFinder
                    {
                        Name = nameof(JobFindersEnum.RabotaBy),
                        LinkTemplate = $"https://rabota.by/search/vacancy/?text={LocationPlaceholder} {SpecialityPlaceholder}&page={PagePlaceholder}",
                        CssClass = "vacancy-info",
                        HrefPrefix = "rabota.by/vacancy/",
                        BaseUrl = "https://rabota.by",
                        LocationTransliteration = nameof(TransliterationEnum.Cyrillic)
                    },
                    new JobFinder
                    {
                        Name = nameof(JobFindersEnum.DevBy),
                        LinkTemplate = $"https://jobs.devby.io/?[city_id][]={LocationPlaceholder}&filter[search]={SpecialityPlaceholder}",
                        CssClass = "vacancies-list-item",
                        HrefPrefix = "/vacancies/",
                        BaseUrl = "https://jobs.devby.io",
                        LocationTransliteration = nameof(TransliterationEnum.Latin),
                        AddBaseUrlToHrefPrefix = true
                    },
                    new JobFinder
                    {
                        Name = nameof(JobFindersEnum.PracaBy),
                        LinkTemplate = $"https://praca.by/rabota-{LocationPlaceholder}/?search%5Bquery%5D={SpecialityPlaceholder}",
                        CssClass = "vac-small__column vac-small__column_2",
                        HrefPrefix = "/vacancy/",
                        BaseUrl = "https://praca.by",
                        LocationTransliteration = nameof(TransliterationEnum.Latin),
                        AddBaseUrlToHrefPrefix = true,
                        MandatoryLocation = true
                    },
                    new JobFinder
                    {
                        Name = nameof(JobFindersEnum.BeBee),
                        LinkTemplate = $"https://bebee.com/by/jobs?q={SpecialityPlaceholder}&location={LocationPlaceholder}",
                        CssClass = "group relative",
                        HrefPrefix = "/by/jobs/",
                        BaseUrl = "https://bebee.com/",
                        LocationTransliteration = nameof(TransliterationEnum.Latin),
                        NodeTag = "article",
                        AddBaseUrlToHrefPrefix = true
                    },
                    new JobFinder
                    {
                        Name = nameof(JobFindersEnum.Joblum),
                        LinkTemplate = $"https://by.joblum.com/jobs?q={SpecialityPlaceholder}&sort=0&lo%5B%5D={LocationPlaceholder}&p={PagePlaceholder}",
                        CssClass = "result-wrp row",
                        HrefPrefix = "/job/",
                        BaseUrl = "https://by.joblum.com/",
                        LocationTransliteration = nameof(TransliterationEnum.Cyrillic),
                        AddBaseUrlToHrefPrefix = true,
                        MandatoryLocation = true
                    },
                    new JobFinder
                    {
                        Name = nameof(JobFindersEnum.LinkedIn),
                        LinkTemplate = $"https://www.linkedin.com/search/results/all/?&keywords={LocationPlaceholder} {SpecialityPlaceholder}",
                        CssClass = "",
                        HrefPrefix = "/job/",
                        BaseUrl = "https://linkedin.com/",
                        LocationTransliteration = nameof(TransliterationEnum.Latin)
                    },
                    new JobFinder
                    {
                        Name = nameof(JobFindersEnum.Trabajo),
                        LinkTemplate = $"https://by.trabajo.org/работы-{SpecialityPlaceholder}/{LocationPlaceholder}",
                        CssClass = "job-item",
                        HrefPrefix = "by.trabajo.org/работа-",
                        BaseUrl = "https://by.trabajo.org/",
                        LocationTransliteration = nameof(TransliterationEnum.Latin),
                        NodeTag = "li",
                    }
                );


                dbContext.SaveChanges();
            }
        }
    }
}
