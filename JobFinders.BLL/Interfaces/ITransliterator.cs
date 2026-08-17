using JobFinders.BLL.Models;

namespace JobFinders.BLL.Interfaces
{
    public interface ITransliterator
    {
        public string Transliterate(string? word, JobFinderSetting? setting);
    }
}
