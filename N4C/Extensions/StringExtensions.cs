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

        public static string GetDisplayName(this string displayName, string propertyName, string culture)
        {
            string result = string.Empty;
            if (displayName.HasNotAny())
                result = propertyName;
            if (displayName.GetCount('{') == 1 && displayName.GetCount('}') == 1 && displayName.GetCount(';') == 1)
            {
                displayName = displayName.Substring(1, displayName.Length - 2);
                var valueParts = displayName.Split(';');
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

        public static string GetErrorMessage(this string errorMessage, string propertyName, string culture)
        {
            string result = string.Empty;
            string displayName;
            string[] valueParts;
            if (errorMessage.HasAny())
            {
                if (errorMessage.Contains("not valid", StringComparison.OrdinalIgnoreCase) || errorMessage.Contains("invalid", StringComparison.OrdinalIgnoreCase))
                {
                    result = culture == Defaults.TR ? "Geçersiz değer" : "Invalid value";
                }
                else if ((errorMessage.GetCount('{') == 0 && errorMessage.GetCount('}') == 0) || (errorMessage.GetCount('{') == 2 && errorMessage.GetCount('}') == 2))
                {
                    if (errorMessage.GetCount('{') == 2 && errorMessage.GetCount('}') == 2)
                    {
                        displayName = errorMessage.Substring(errorMessage.IndexOf('{'), errorMessage.IndexOf('}') + 1);
                        errorMessage = errorMessage.Replace(displayName, GetDisplayName(displayName, propertyName, culture));
                    }
                    if (errorMessage.GetCount(';') == 1)
                    {
                        valueParts = errorMessage.Split(';');
                        if (culture == Defaults.TR)
                            result = valueParts.First();
                        else
                            result = valueParts.Last();
                    }
                    else
                    {
                        result = errorMessage;
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

        public static Uri GetUri(this string uri, UriKind uriKind)
        {
            Uri uriResult = null;
            bool result = uri.HasAny() && Uri.TryCreate(uri, uriKind, out uriResult);
            return result ? uriResult : null;
        }

        public static Uri GetUri(this string uriDictionaryKey)
        {
            Uri uriResult = null;
            if (uriDictionaryKey.HasAny() && Settings.UriDictionary is not null && Settings.UriDictionary.Any())
            {
                foreach (var item in Settings.UriDictionary)
                {
                    if (item.Key.ToLower() == uriDictionaryKey.ToLower() && Uri.TryCreate(item.Value, UriKind.Absolute, out uriResult))
                        break;
                }
            }
            return uriResult;
        }
    }
}
