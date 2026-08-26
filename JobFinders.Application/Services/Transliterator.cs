using System.Globalization;

using JobFinders.Application.Enums;
using JobFinders.Domain.Interfaces;
using JobFinders.Domain.Models;

using NickBuhro.Translit;

namespace JobFinders.Application.Services
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
