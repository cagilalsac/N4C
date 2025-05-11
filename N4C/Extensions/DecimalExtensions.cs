using N4C.App;
using System.Globalization;
using System.Text;

namespace N4C.Extensions
{
    public static class DecimalExtensions
    {
        /// <summary>
        /// Supports to a maximum value of 10 trillion. Automatically rounds decimals to 2 digits. Only supports Turkish for now.
        /// </summary>
        [Obsolete]
        public static string ToMoneyStringV1(this decimal value, string culture = default, bool adjoint = false)
        {
            if (string.IsNullOrWhiteSpace(culture) || culture != Cultures.TR)
                return "";
            if (value > 10000000000000.0m)
                return "";
            string currency = "TL";
            if (value == 0)
                return "SIFIR " + currency;
            string decimalPoint = ",";
            string result = "";
            string sAmount = value.ToString("F2").Replace('.', ',');
            string wholePart = sAmount.Substring(0, sAmount.IndexOf(','));
            string decimalPart = sAmount.Substring(sAmount.IndexOf(',') + 1, 2);
            string[] ones;
            string[] tens;
            string[] thousands;
            if (adjoint)
            {
                ones = ["", "BİR", "İKİ", "ÜÇ", "DÖRT", "BEŞ", "ALTI", "YEDİ", "SEKİZ", "DOKUZ"];
                tens = ["", "ON", "YİRMİ", "OTUZ", "KIRK", "ELLİ", "ALTMIŞ", "YETMİŞ", "SEKSEN", "DOKSAN"];
                thousands = ["KATRİLYON", "TRİLYON", "MİLYAR", "MİLYON", "BİN", ""];
            }
            else
            {
                ones = ["", "BİR ", "İKİ ", "ÜÇ ", "DÖRT ", "BEŞ ", "ALTI ", "YEDİ ", "SEKİZ ", "DOKUZ "];
                tens = ["", "ON ", "YİRMİ ", "OTUZ ", "KIRK ", "ELLİ ", "ALTMIŞ ", "YETMİŞ ", "SEKSEN ", "DOKSAN "];
                thousands = ["KATRİLYON ", "TRİLYON ", "MİLYAR ", "MİLYON ", "BİN ", ""];
            }
            int groupCount = 6;
            wholePart = wholePart.PadLeft(groupCount * 3, '0');
            string groupValue;
            for (int i = 0; i < groupCount * 3; i += 3)
            {
                groupValue = "";
                if (wholePart.Substring(i, 1) != "0")
                {
                    if (adjoint)
                        groupValue += ones[Convert.ToInt32(wholePart.Substring(i, 1))] + "YÜZ";
                    else
                        groupValue += ones[Convert.ToInt32(wholePart.Substring(i, 1))] + "YÜZ ";
                }
                if (groupValue.Trim() == "BİRYÜZ" || groupValue.Trim() == "BİR YÜZ")
                    groupValue = adjoint == true ? "YÜZ" : "YÜZ ";
                groupValue += tens[Convert.ToInt32(wholePart.Substring(i + 1, 1))];
                groupValue += ones[Convert.ToInt32(wholePart.Substring(i + 2, 1))];
                if (groupValue != "")
                    groupValue += thousands[i / 3];
                if (groupValue.Trim() == "BİRBİN" || groupValue.Trim() == "BİR BİN")
                    groupValue = adjoint == true ? "BİN" : "BİN ";
                result += groupValue;
            }
            if (Convert.ToInt64(wholePart) != 0)
            {
                if (currency.Trim().ToUpper().Equals("TL"))
                {
                    if (adjoint)
                        result += " " + currency;
                    else
                        result += currency;
                }
                result = result.Trim();
            }
            else
            {
                result = "";
            }
            if (Convert.ToInt64(decimalPart) != 0)
            {
                if (currency.Trim().ToUpper().Equals("TL"))
                {
                    result += " ";
                }
                else
                {
                    if (decimalPoint.Trim().Equals(","))
                    {
                        if (adjoint)
                            result += "VİRGÜL";
                        else
                            result += " VİRGÜL ";
                    }
                    else
                    {
                        if (adjoint)
                            result += "NOKTA";
                        else
                            result += " NOKTA ";
                    }
                }
                if (decimalPart.Substring(0, 1) != "0")
                    result += tens[Convert.ToInt32(decimalPart.Substring(0, 1))];
                if (decimalPart.Substring(1, 1) != "0")
                    result += ones[Convert.ToInt32(decimalPart.Substring(1, 1))];
                result = result.Trim();
                if (currency.Trim().ToUpper().Equals("TL"))
                    result += " kr.";
                else
                    result += " " + currency;
            }
            else
            {
                if (!currency.Trim().ToUpper().Equals("TL"))
                    result += " " + currency;
            }
            return result;
        }

        /// <summary>
        /// Converts a decimal value to its textual money representation. Supports to a maximum value of 10 trillion.
        /// </summary>
        public static string ToMoneyString(this decimal value, string culture = default, bool adjoint = false)
        {
            if (value > 10_000_000_000_000.0m)
                return "";
            if (string.IsNullOrWhiteSpace(culture))
                culture = Settings.Culture;
            string currency = culture == Cultures.TR ? "TL" : "USD";
            if (value == 0)
                return culture == Cultures.TR ? "SIFIR " + currency : "ZERO " + currency;
            string[] ones, tens, thousands;
            string decimalPoint;
            if (culture == Cultures.TR)
            {
                ones = adjoint ? ["", "BİR", "İKİ", "ÜÇ", "DÖRT", "BEŞ", "ALTI", "YEDİ", "SEKİZ", "DOKUZ"] :
                                 ["", "BİR ", "İKİ ", "ÜÇ ", "DÖRT ", "BEŞ ", "ALTI ", "YEDİ ", "SEKİZ ", "DOKUZ "];
                tens = adjoint ? ["", "ON", "YİRMİ", "OTUZ", "KIRK", "ELLİ", "ALTMIŞ", "YETMİŞ", "SEKSEN", "DOKSAN"] :
                                 ["", "ON ", "YİRMİ ", "OTUZ ", "KIRK ", "ELLİ ", "ALTMIŞ ", "YETMİŞ ", "SEKSEN ", "DOKSAN "];
                thousands = adjoint ? ["KATRİLYON", "TRİLYON", "MİLYAR", "MİLYON", "BİN", ""] :
                                      ["KATRİLYON ", "TRİLYON ", "MİLYAR ", "MİLYON ", "BİN ", ""];
                decimalPoint = adjoint ? "VİRGÜL" : " VİRGÜL ";
            }
            else
            {
                ones = adjoint ? ["", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE"] :
                                 ["", "ONE ", "TWO ", "THREE ", "FOUR ", "FIVE ", "SIX ", "SEVEN ", "EIGHT ", "NINE "];
                tens = adjoint ? ["", "TEN", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY"] :
                                 ["", "TEN ", "TWENTY ", "THIRTY ", "FORTY ", "FIFTY ", "SIXTY ", "SEVENTY ", "EIGHTY ", "NINETY "];
                thousands = adjoint ? ["QUADRILLION", "TRILLION", "BILLION", "MILLION", "THOUSAND", ""] :
                                      ["QUADRILLION ", "TRILLION ", "BILLION ", "MILLION ", "THOUSAND ", ""];
                decimalPoint = adjoint ? "POINT" : " POINT ";
            }
            var result = new StringBuilder();
            var money = value.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',');
            var wholePart = money.Substring(0, money.IndexOf(',')).PadLeft(18, '0'); // Pad for six groups (thousands)
            var decimalPart = money.Substring(money.IndexOf(',') + 1, 2);
            for (int i = 0; i < 18; i += 3)
            {
                var groupValue = "";
                if (wholePart[i] != '0')
                    groupValue += ones[int.Parse(wholePart[i].ToString())] + 
                        (culture == Cultures.TR ? (adjoint ? "YÜZ" : "YÜZ ") : (adjoint ? "HUNDRED" : "HUNDRED "));
                groupValue += tens[int.Parse(wholePart[i + 1].ToString())];
                groupValue += ones[int.Parse(wholePart[i + 2].ToString())];
                if (!string.IsNullOrWhiteSpace(groupValue))
                    groupValue += thousands[i / 3];
                result.Append(groupValue);
            }
            if (decimalPart != "00")
            {
                result.Append(decimalPoint);
                if (decimalPart[0] != '0')
                    result.Append(tens[int.Parse(decimalPart[0].ToString())]);
                if (decimalPart[1] != '0')
                    result.Append(ones[int.Parse(decimalPart[1].ToString())]);
                result.Append(culture == Cultures.TR ? " kr." : " cents");
            }
            return result.ToString().Trim() + " " + currency;
        }
    }
}
