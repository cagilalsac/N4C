using N4C.Attributes;
using System.Text.Json.Serialization;

namespace N4C.Models
{
    public class FileResponse : Response
    {
        [JsonIgnore, ExcelIgnore]
        public Stream FileStream { get; set; }

        [JsonIgnore, ExcelIgnore]
        public string FileContentType { get; set; }

        [JsonIgnore, ExcelIgnore]
        public string FileName { get; set; }

        [ExcelIgnore, DisplayName("Dosyalar", "Files")]
        public virtual string MainFile { get; set; }

        [ExcelIgnore]
        public virtual List<string> OtherFiles { get; set; }
    }
}
