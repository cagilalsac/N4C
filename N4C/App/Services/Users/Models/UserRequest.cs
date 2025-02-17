using N4C.Attributes;

namespace N4C.App.Services.Users.Models
{
    public class UserRequest : Request
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        [DisplayName("Kullanıcı Adı")]
        public string UserName { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 3)]
        [DisplayName("Şifre")]
        public string Password { get; set; }

        [DisplayName("Şifre Onay")]
        public string ConfirmPassword { get; set; }

        public bool Active { get; set; }

        [Required]
        [DisplayName("Roller")]
        public List<int> RoleIds { get; set; }
    }
}
