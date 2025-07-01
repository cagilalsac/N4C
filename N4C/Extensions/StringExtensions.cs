using N4C.Models;
using System.Web;

namespace N4C.Extensions
{
    public static class StringExtensions
    {
        public static string HasNotAny(this string value, string defaultValue)
        {
            return HasNotAny(value) ? defaultValue : value;
        }

        public static bool HasNotAny(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool HasAny(this string value)
        {
            return !HasNotAny(value);
        }

        public static string FirstLetterToUpperOthersToLower(this string value)
        {
            string result = string.Empty;
            if (value.HasAny())
            {
                if (value.Contains(' '))
                {
                    string[] valueParts = value.Split(' ');
                    foreach (string valuePart in valueParts)
                    {
                        if (valuePart.HasAny())
                        {
                            result += valuePart.Substring(0, 1).ToUpper();
                            if (valuePart.Length > 1)
                                result += valuePart.Substring(1).ToLower();
                            result += " ";
                        }
                    }
                    result = result.TrimEnd();
                }
                else
                {
                    result = value.Substring(0, 1).ToUpper();
                    if (value.Length > 1)
                        result += value.Substring(1).ToLower();
                }
            }
            return result;
        }

        public static int GetCount(this string value, char character)
        {
            if (value.HasAny())
                return value.Count(v => v == character);
            return 0;
        }

        public static string SeperateUpperCaseCharacters(this string value, char seperator = ' ')
        {
            string result = string.Empty;
            if (value.HasNotAny())
                return result;
            result += value[0];
            for (int i = 1; i < value.Length; i++)
            {
                if (char.IsUpper(value[i]) && !char.IsUpper(value[i - 1]) && char.IsLetter(value[i - 1]))
                    result += seperator;
                result += value[i];
            }
            return result;
        }

        public static string GetDisplayName(this string value, string propertyName, string culture)
        {
            string result = string.Empty;
            if (value.HasNotAny())
                result = propertyName;
            if (value.GetCount('{') == 1 && value.GetCount('}') == 1 && value.GetCount(';') == 1)
            {
                value = value.Substring(1, value.Length - 2);
                var valueParts = value.Split(';');
                if (culture == Defaults.TR)
                {
                    result = valueParts.First();
                }
                else
                {
                    result = valueParts.Last();
                    if (result.HasNotAny() && propertyName.HasAny())
                    {
                        result = propertyName;
                        if (result.Contains('.'))
                            result = result.Split('.').Last();
                        result = result.SeperateUpperCaseCharacters();
                    }
                }
            }
            return result.Contains('_') ? result.Remove(result.IndexOf('_')) : result;
        }

        public static string GetErrorMessage(this string value, string propertyName, string culture)
        {
            string result = string.Empty;
            string displayName;
            string[] valueParts;
            if (value.HasAny())
            {
                if (value.Contains("not valid", StringComparison.OrdinalIgnoreCase) || value.Contains("invalid", StringComparison.OrdinalIgnoreCase))
                {
                    result = culture == Defaults.TR ? "Geçersiz değer" : "Invalid value";
                }
                else if ((value.GetCount('{') == 0 && value.GetCount('}') == 0) || (value.GetCount('{') == 2 && value.GetCount('}') == 2))
                {
                    if (value.GetCount('{') == 2 && value.GetCount('}') == 2)
                    {
                        displayName = value.Substring(value.IndexOf('{'), value.IndexOf('}') + 1);
                        value = value.Replace(displayName, GetDisplayName(displayName, propertyName, culture));
                    }
                    if (value.GetCount(';') == 1)
                    {
                        valueParts = value.Split(';');
                        if (culture == Defaults.TR)
                            result = valueParts.First();
                        else
                            result = valueParts.Last();
                    }
                    else
                    {
                        result = value;
                    }
                }
                if (!result.EndsWith("!"))
                    result += "!";
            }
            return result;
        }

        public static string GetQueryStringValue(this string uri, string key)
        {
            var queryString = HttpUtility.ParseQueryString(new Uri(uri).Query);
            return queryString[key];
        }

        public static Uri GetUri(this string value)
        {
            Uri uri = null;
            bool result = value.HasAny() && Uri.TryCreate(value, UriKind.Absolute, out uri);
            return result ? uri : null;
        }
    }
}
