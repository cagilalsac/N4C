using N4C.Attributes;
using System.Text.Json.Serialization;

namespace N4C.App.Services.Auth.Models
{
    public class LoginRequest : Request
    {
        [JsonIgnore]
        public override int Id { get => base.Id; set => base.Id = value; }

        [JsonIgnore]
        public override string Guid { get => base.Guid; set => base.Guid = value; }

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
