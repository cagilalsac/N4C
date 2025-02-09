using N4C.Attributes;
using System.Text.Json.Serialization;

namespace N4C.Domain
{
    public abstract class Entity
    {
        [ExcelIgnore]
        public int Id { get; set; }

        [ExcelIgnore, JsonIgnore]
        public string Guid { get; set; }
    }
}
