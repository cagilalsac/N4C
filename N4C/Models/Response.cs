using N4C.Attributes;
using N4C.Domain;

namespace N4C.Models
{
    public class Response : Data
    {
        [DisplayName("Oluşturan", "Created By")]
        public virtual string CreateDate_ { get; set; }

        [DisplayName("Güncelleyen", "Updated By")]
        public virtual string UpdateDate_ { get; set; }
    }
}
