using N4C.Attributes;

namespace N4C.Domain
{
    public abstract class Entity
    {
        [ExcelIgnore]
        public int Id { get; set; }

        [ExcelIgnore]
        public string Guid { get; set; }
    }
}
