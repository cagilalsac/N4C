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

        [DisplayName("Aktif")]
        public bool Active { get; set; } = true;

        [Required]
        [DisplayName("Roller", "Roles")]
        public List<int> RoleIds { get; set; } = new List<int>();
    }
}
