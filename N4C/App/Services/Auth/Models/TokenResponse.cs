using System.Text.Json.Serialization;

namespace N4C.App.Services.Auth.Models
{
    public class TokenResponse : Response
    {
        [JsonIgnore]
        public override int Id { get => base.Id; set => base.Id = value; }

        [JsonIgnore]
        public override string Guid { get => base.Guid; set => base.Guid = value; }

        public string Token { get; set; }
    }
}
