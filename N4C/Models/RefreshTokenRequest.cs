using N4C.Attributes;
using System.Text.Json.Serialization;

namespace N4C.Models
{
    public class RefreshTokenRequest : Request
    {
        [JsonIgnore]
        public override int Id { get => base.Id; set => base.Id = value; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
