using N4C.Attributes;
using N4C.Domain;
using System.Text.Json.Serialization;

namespace N4C.Models
{
    public class Response : Data
    {
        [DisplayName("Oluşturulma Tarihi", "Create Date")]
        public DateTime? CreateDate { get; set; }

        [DisplayName("Oluşturan", "Created By")]
        [JsonIgnore]
        public virtual string CreateDate_ { get; set; }

        [DisplayName("Güncellenme Tarihi", "Update Date")]
        public DateTime? UpdateDate { get; set; }

        [DisplayName("Güncelleyen", "Updated By")]
        [JsonIgnore]
        public virtual string UpdateDate_ { get; set; }
    }
}
