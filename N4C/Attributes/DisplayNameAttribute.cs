namespace N4C.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DisplayNameAttribute : System.ComponentModel.DisplayNameAttribute
    {
        public DisplayNameAttribute(string tr, string en = default)
        {
            DisplayNameValue = "{" + tr + ";" + (string.IsNullOrWhiteSpace(en) ? string.Empty : en) + "}";
        }
    }
}
