using N4C.Attributes;
using N4C.Domain;

namespace N4C.Models
{
    public class Response : Data
    {
        [DisplayName("Oluşturulma Tarihi")]
        public virtual string CreateDateS { get; set; }

        [DisplayName("Oluşturan")]
        public virtual string CreatedBy { get; set; }

        [DisplayName("Güncellenme Tarihi")]
        public virtual string UpdateDateS { get; set; }

        [DisplayName("Güncelleyen")]
        public virtual string UpdatedBy { get; set; }
    }
}
