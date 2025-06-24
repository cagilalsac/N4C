using N4C.Extensions;

namespace N4C.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DisplayNameAttribute : System.ComponentModel.DisplayNameAttribute
    {
        public DisplayNameAttribute(string tr, string en = default)
        {
            DisplayNameValue = "{" + tr + ";" + en.HasNotAny(string.Empty) + "}";
        }
    }
}
