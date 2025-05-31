using N4C.Attributes;
using N4C.Models;

namespace N4C.Users.App.Models
{
    public class N4CUserResponse : Response
    {
        [DisplayName("Kullanıcı Adı")]
        public string UserName { get; set; }

        [DisplayName("Şifre")]
        public string Password { get; set; }

        [DisplayName("E-Posta", "E-Mail")]
        public string Email { get; set; }

        [DisplayName("Adı")]
        public string FirstName { get; set; }

        [DisplayName("Soyadı")]
        public string LastName { get; set; }

        [DisplayName("Tam Adı")]
        public string FullName { get; set; }

        [DisplayName("Roller")]
        public string Roles { get; set; }

        [DisplayName("Roller", "Roles")]
        public List<int> RoleIds { get; set; }

        [DisplayName("Durum")]
        public N4CStatusResponse Status { get; set; }

        [DisplayName("Durum", "Status")]
        public bool Active { get; set; }

        [DisplayName("Durum", "Status")]
        public string ActiveS { get; set; }
    }
}
