namespace N4C.Domain
{
    public abstract class FileEntity : Entity
    {
        public string MainFile { get; set; }
        public List<string> OtherFiles { get; set; }
    }
}
