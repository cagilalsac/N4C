using N4C.Attributes;
using N4C.Models;

namespace N4C.User.App.Models
{
    public class N4CUserLoginRequest : Request
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        [DisplayName("Kullanıcı Adı")]
        public string UserName { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 3)]
        [DisplayName("Şifre")]
        public string Password { get; set; }
    }
}
