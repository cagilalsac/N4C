using N4C.Attributes;

namespace N4C.Domain
{
    public abstract class Entity
    {
        [ExcelIgnore]
        public virtual int Id { get; set; }

        [ExcelIgnore]
        public virtual string Guid { get; set; }
    }
}
