using N4C.Attributes;
using N4C.Models;

namespace N4C.User.App.Models
{
    public class N4CRoleResponse : Response
    {
        [DisplayName("Rol Adı", "Role Name")]
        public string Name { get; set; }

        [DisplayName("Kullanıcı Sayısı")]
        public int UsersCount { get; set; }

        [DisplayName("Kullanıcılar")]
        public List<N4CUserResponse> Users { get; set; }
    }
}
