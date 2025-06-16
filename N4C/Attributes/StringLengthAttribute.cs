namespace N4C.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class StringLengthAttribute : System.ComponentModel.DataAnnotations.StringLengthAttribute
    {
        public StringLengthAttribute(int maximumLength) : base(maximumLength)
        {
        }

        public override bool IsValid(object value)
        {
            var valid = base.IsValid(value);
            if (!valid)
            {
                string valueAsString = Convert.ToString(value) ?? "0";
                string errorMessageEN, errorMessageTR;
                if (MinimumLength > 0)
                {
                    errorMessageEN = "{0} must have minimum " + MinimumLength + " maximum " + MaximumLength + " characters,";
                    errorMessageTR = "{0} en az " + MinimumLength + " en çok " + MaximumLength + " karakter olmalıdır,";
                }
                else
                {
                    errorMessageEN = "{0} must have maximum " + MaximumLength + " characters,";
                    errorMessageTR = "{0} en çok " + MaximumLength + " karakter olmalıdır,";
                }
                errorMessageEN += $" {valueAsString.Length} characters entered";
                errorMessageTR += $" {valueAsString.Length} karakter girilmiştir";
                ErrorMessage = $"{errorMessageTR};{errorMessageEN}";
            }
            return valid;
        }
    }
}
