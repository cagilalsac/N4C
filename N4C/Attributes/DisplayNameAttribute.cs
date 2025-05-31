namespace N4C.Attributes
{
    public class DisplayNameAttribute : System.ComponentModel.DisplayNameAttribute
    {
        public DisplayNameAttribute(string tr, string en = default)
        {
            DisplayNameValue = "{" + tr + ";" + (string.IsNullOrWhiteSpace(en) ? string.Empty : en) + "}";
        }
    }
}
