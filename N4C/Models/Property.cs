using N4C.Extensions;

namespace N4C.Models
{
    public class Property
    {
        public string Name { get; }
        public string DisplayName { get; }
        public object Value { get; }

        public Property(string name, object value, string displayName = default)
        {
            Name = name;
            Value = value;
            DisplayName = displayName.HasNotAny(string.Empty);
        }
    }
}
