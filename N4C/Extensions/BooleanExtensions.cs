using N4C.Models;

namespace N4C.Extensions
{
    public static class BooleanExtensions
    {
        public static string ToHtml(this bool value, string trueHtml = default, string falseHtml = default, string culture = default)
        {
            return value ? (string.IsNullOrWhiteSpace(trueHtml) ?
                ((culture ?? Cultures.TR) == Cultures.TR ? "Evet" : "Yes") : trueHtml) :
                    (string.IsNullOrWhiteSpace(falseHtml) ?
                        ((culture ?? Cultures.TR) == Cultures.TR ? "Hayır" : "No") : falseHtml);
        }
    }
}
