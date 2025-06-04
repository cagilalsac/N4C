using Microsoft.AspNetCore.Http;
using N4C.Attributes;

namespace N4C.Models
{
    public abstract class FileRequest : Request
    {
        [DisplayName("Ana Dosya", "Main File")]
        public virtual IFormFile MainFormFile { get; set; }

        [DisplayName("Diğer Dosyalar", "Other Files")]
        public virtual List<IFormFile> OtherFormFiles { get; set; }

        /// <summary>
        /// For only getting the main file path in MVC edit operation. Use MainFormFile to post file data with MVC.
        /// </summary>
        public string MainFile { get; set; }
    }
}
