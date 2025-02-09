using N4C.Domain;

namespace N4C.App
{
    public interface IFileResponse : IFile
    {
    }

    public class FileResponse : IFileResponse
    {
        public Stream Stream { get; }
        public string ContentType { get; }
        public string Name { get; }
        public string MainFile { get; set; }
        public List<string> OtherFiles { get; set; }

        public FileResponse(Stream stream, string contentType, string name)
        {
            Stream = stream;
            ContentType = contentType;
            Name = name;
        }

        public FileResponse(string mainFile, List<string> otherFiles = default)
        {
            MainFile = mainFile;
            OtherFiles = otherFiles;
        }
    }
}
