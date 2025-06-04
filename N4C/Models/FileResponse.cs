using N4C.Attributes;
using System.Text.Json.Serialization;

namespace N4C.Models
{
    public class FileResponse : Response
    {
        [JsonIgnore, ExcelIgnore]
        public Stream FileStream { get; set; }

        [ExcelIgnore]
        public string FileContentType { get; set; }

        [ExcelIgnore]
        public string FileName { get; set; }

        [ExcelIgnore, DisplayName("Ana Dosya")]
        public virtual string MainFile { get; set; }

        [ExcelIgnore, DisplayName("Diğer Dosyalar")]
        public virtual List<string> OtherFiles { get; set; }
    }
}
