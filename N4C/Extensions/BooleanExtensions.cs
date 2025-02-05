namespace N4C.Extensions
{
    public static class BooleanExtensions
    {
        public static string ToHtml(this bool value, string trueHtml = default, string falseHtml = default)
        {
            return value ? (string.IsNullOrWhiteSpace(trueHtml) ? "True" : trueHtml) : 
                (string.IsNullOrWhiteSpace(falseHtml) ? "False" : falseHtml);
        }
    }
}
