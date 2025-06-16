using N4C.Attributes;
using N4C.Models;
using System.Text.Json.Serialization;

namespace N4C.Users.App.Models
{
    public class N4CRefreshTokenRequest : Request
    {
        [JsonIgnore]
        public override int Id { get => base.Id; set => base.Id = value; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }

        public bool SlidingExpiration { get; set; } = true;
    }
}
