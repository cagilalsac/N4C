using N4C.Models;
using System.Text.Json.Serialization;

namespace N4C.Users.App.Models
{
    public class N4CTokenResponse : Response
    {
        [JsonIgnore]
        public override DateTime? UpdateDate { get => base.UpdateDate; set => base.UpdateDate = value; }

        public string Token { get; set; }

        public string BearerToken { get; set; }

        public string RefreshToken { get; set; }
    }
}
