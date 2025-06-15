using N4C.Attributes;
using N4C.Models;
using System.Text.Json.Serialization;

namespace N4C.Users.App.Models
{
    public class N4CUserLoginRequest : Request
    {
        [JsonIgnore]
        public override int Id { get => base.Id; set => base.Id = value; }

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
