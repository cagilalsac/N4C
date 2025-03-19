using System.Text.Json.Serialization;

namespace N4C.App.Services.Auth.Models
{
    public class JwtResponse : Response
    {
        [JsonIgnore]
        public override int Id { get => base.Id; set => base.Id = value; }

        public string Token { get; set; }
        public DateTime? Expiration { get; set; }
    }
}
