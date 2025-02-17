using N4C.Attributes;
using N4C.Domain;
using System.Text.Json.Serialization;

namespace N4C.App.Services.Users.Models
{
    public class UserResponse : Response, IModified
    {
        [DisplayName("Kullanıcı Adı")]
        public string UserName { get; set; }

        [DisplayName("Şifre")]
        [JsonIgnore]
        public string Password { get; set; }

        public bool Active { get; set; }

        [DisplayName("Durum")]
        [JsonIgnore]
        public string ActiveS { get; set; }

        [DisplayName("Roller")]
        public List<RoleResponse> Roles { get; set; }

        [DisplayName("Oluşturulma Tarihi")]
        [JsonIgnore]
        public DateTime CreateDate { get; set; }

        [DisplayName("Oluşturan")]
        [JsonIgnore]
        public string CreatedBy { get; set; }

        [DisplayName("Güncellenme Tarihi")]
        [JsonIgnore]
        public DateTime? UpdateDate { get; set; }

        [DisplayName("Güncelleyen")]
        [JsonIgnore]
        public string UpdatedBy { get; set; }
    }
}
