namespace N4C.Domain
{
    public interface IFile
    {
        public string MainFile { get; set; }
        public List<string> OtherFiles { get; set; }
    }
}
