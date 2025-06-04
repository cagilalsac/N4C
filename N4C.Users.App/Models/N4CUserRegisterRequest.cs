using N4C.Attributes;
using N4C.Models;

namespace N4C.Users.App.Models
{
    public class N4CUserRegisterRequest : Request
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        [DisplayName("Kullanıcı Adı")]
        public string UserName { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 3)]
        [DisplayName("Şifre")]
        public string Password { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 3)]
        [DisplayName("Şifre Onay")]
        public string ConfirmPassword { get; set; }

        [StringLength(200, MinimumLength = 5)]
        [DisplayName("E-Posta", "E-Mail")]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(75)]
        [DisplayName("Adı")]
        public string FirstName { get; set; }

        [StringLength(75)]
        [DisplayName("Soyadı")]
        public string LastName { get; set; }
    }
}
