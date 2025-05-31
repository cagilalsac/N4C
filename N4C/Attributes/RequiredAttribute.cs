namespace N4C.Attributes
{
    public class RequiredAttribute : System.ComponentModel.DataAnnotations.RequiredAttribute
    {
        public RequiredAttribute()
        {
        }

        public override bool IsValid(object value)
        {
            var valid = base.IsValid(value);
            if (valid && value is not null && value is List<int> && !(value as List<int>).Any())
                valid = false;
            if (!valid)
                ErrorMessage = "{0} zorunludur;{0} is required";
            return valid;
        }
    }
}
