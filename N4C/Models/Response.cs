using N4C.Attributes;
using N4C.Domain;

namespace N4C.Models
{
    public class Response : Data
    {
        [DisplayName("Oluşturulma Tarihi", "Create Date")]
        [ExcelIgnore]
        public virtual DateTime? CreateDate { get; set; }

        [DisplayName("Oluşturan", "Created By")]
        public virtual string CreateDate_ { get; set; }

        [DisplayName("Güncellenme Tarihi", "Update Date")]
        [ExcelIgnore]
        public virtual DateTime? UpdateDate { get; set; }

        [DisplayName("Güncelleyen", "Updated By")]
        public virtual string UpdateDate_ { get; set; }
    }
}
