using N4C.Models;

namespace N4C.Extensions
{
    public static class StringExtensions
    {
        public static int GetCount(this string value, char character)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Count(v => v == character);
            return 0;
        }

        public static string SeperateUpperCaseCharacters(this string value, char seperator = ' ')
        {
            string result = string.Empty;
            if (string.IsNullOrWhiteSpace(value))
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
            if (string.IsNullOrWhiteSpace(value))
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
                    if (string.IsNullOrWhiteSpace(result) && !string.IsNullOrWhiteSpace(propertyName))
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
            if (!string.IsNullOrWhiteSpace(value))
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
    }
}
