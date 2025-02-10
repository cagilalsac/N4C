using Microsoft.AspNetCore.Http;
using N4C.Attributes;

namespace N4C.App
{
    public abstract class FileRequest : Request
    {
        [DisplayName("Ana Dosya", "Main File")]
        public virtual IFormFile MainFormFile { get; set; }

        [DisplayName("Diğer Dosyalar", "Other Files")]
        public virtual List<IFormFile> OtherFormFiles { get; set; }

        public string _MainFile { get; set; }
    }
}
