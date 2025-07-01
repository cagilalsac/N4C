using System.Text.Json.Serialization;

namespace N4C.Models
{
    public class TokenResponse : Response
    {
        [JsonIgnore]
        public override DateTime? UpdateDate { get => base.UpdateDate; set => base.UpdateDate = value; }

        public string Token { get; set; }

        public string BearerToken { get; set; }

        public string RefreshToken { get; set; }
    }
}
