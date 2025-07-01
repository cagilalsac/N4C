using N4C.Domain;
using System.Text.Json.Serialization;

namespace N4C.Models
{
    public class Request : Data
    {
        [JsonIgnore]
        public override string Guid { get => base.Guid; set => base.Guid = value; }
    }
}
