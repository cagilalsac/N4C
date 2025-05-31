using N4C.Attributes;
using N4C.Domain;

namespace N4C.Models
{
    public class Response : Data
    {
        [DisplayName("Oluşturulma Tarihi")]
        public string CreateDateS { get; set; }

        [DisplayName("Oluşturan")]
        public string CreatedBy { get; set; }

        [DisplayName("Güncellenme Tarihi")]
        public string UpdateDateS { get; set; }

        [DisplayName("Güncelleyen")]
        public string UpdatedBy { get; set; }
    }
}
