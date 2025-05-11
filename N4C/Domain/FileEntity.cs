namespace N4C.Domain
{
    public abstract class FileEntity : Entity
    {
        public virtual string MainFile { get; set; }
        public virtual List<string> OtherFiles { get; set; }
    }
}
