using JobFinders.Domain.Models;

namespace JobFinders.Domain.Interfaces
{
    public interface ITransliterator
    {
        public string Transliterate(string? word, JobFinderSetting? setting);
    }
}
