using Microsoft.AspNetCore.Http;

namespace N4C.App
{
    public interface IFileRequest
    {
        public IFormFile MainFormFile { get; set; }
        public List<IFormFile> OtherFormFiles { get; set; }
        public string MainFile { get; set; }
    }
}
