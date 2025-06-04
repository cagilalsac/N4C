using System.ComponentModel.DataAnnotations;

namespace N4C.Attributes
{
    public class EmailAddressAttribute : DataTypeAttribute
    {
        public EmailAddressAttribute() : base(DataType.EmailAddress)
        {
        }

        public override bool IsValid(object value)
        {
            var valid = base.IsValid(value);
            if (!valid)
                ErrorMessage = "{0} e-posta formatında olmalıdır;{0} must be in e-mail format";
            return valid;
        }
    }
}
