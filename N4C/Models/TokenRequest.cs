using N4C.Attributes;
using System.Text.Json.Serialization;

namespace N4C.Models
{
    public class TokenRequest : Request
    {
        [JsonIgnore]
        public override int Id { get => base.Id; set => base.Id = value; }

        [Required]
        public virtual string UserName { get; set; }

        [Required]
        public virtual string Password { get; set; }
    }
}
