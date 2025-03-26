using N4C.Attributes;
using N4C.Domain;
using System.Text.Json.Serialization;

namespace N4C.App.Services.Users.Models
{
    public class RoleResponse : Response, IModified
    {
        [DisplayName("Role Adı", "Role Name")]
        public string Name { get; set; }

        [DisplayName("Kullanıcı Sayısı")]
        public int UsersCount { get; set; }

        [DisplayName("Kullanıcılar")]
        public List<UserResponse> Users { get; set; }

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
