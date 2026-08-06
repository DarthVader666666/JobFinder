using System.Globalization;

using JobFinders.BLL.Enums;
using JobFinders.BLL.Interfaces;
using JobFinders.BLL.Models;

using NickBuhro.Translit;

namespace JobFinders.BLL.Services
{
    public class Transliterator: ITransliterator
    {
        public string Transliterate(string? location, JobFinderSetting? setting)
        {
            var transliteration = Enum.Parse<TransliterationEnum>(setting?.LocationTransliteration ?? "");

            location = string.IsNullOrEmpty(location) && (setting?.MandatoryLocation ?? false) ? "minsk" : location;
            location = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(location ?? "");

            location = transliteration switch
            {
                TransliterationEnum.Latin => Transliteration.CyrillicToLatin(location),
                TransliterationEnum.Cyrillic => Transliteration.LatinToCyrillic(location),
                _ => location
            };

            if (setting?.ConvertLocation ?? false)
            {
                location = setting?.LocationDictionary?.FirstOrDefault(x => location?.Contains(x.Key, StringComparison.InvariantCultureIgnoreCase) ?? false).Value ?? string.Empty;
            }

            return location;
        }
    }
}
