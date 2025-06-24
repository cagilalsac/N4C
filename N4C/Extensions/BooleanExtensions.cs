using N4C.Models;

namespace N4C.Extensions
{
    public static class BooleanExtensions
    {
        public static string ToHtml(this bool value, string trueHtml = default, string falseHtml = default, string culture = default)
        {
            return value ? (trueHtml.HasNotAny() ?
                (culture.HasNotAny(Settings.Culture) == Defaults.TR ? "Evet" : "Yes") : trueHtml) :
                    (falseHtml.HasNotAny() ?
                        (culture.HasNotAny(Settings.Culture) == Defaults.TR ? "Hayır" : "No") : falseHtml);
        }
    }
}
