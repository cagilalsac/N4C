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
        public string Password { get; set; }

        public bool Active { get; set; }

        public List<int> RoleIds { get; set; }

        [DisplayName("Aktif")]
        public string ActiveS { get; set; }

        [DisplayName("Roller")]
        public List<RoleResponse> Roles { get; set; }

        [DisplayName("Oluşturulma Tarihi")]
        public DateTime CreateDate { get; set; }

        [DisplayName("Oluşturan")]
        public string CreatedBy { get; set; }

        [DisplayName("Güncellenme Tarihi")]
        public DateTime? UpdateDate { get; set; }

        [DisplayName("Güncelleyen")]
        public string UpdatedBy { get; set; }
    }
}
