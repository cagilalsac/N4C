using N4C.Attributes;
using System.Text.Json.Serialization;

namespace N4C.App.Services.Files.Models
{
    public class FileResponse : Response
    {
        [JsonIgnore, ExcelIgnore]
        public Stream FileStream { get; set; }

        [JsonIgnore, ExcelIgnore]
        public string FileContentType { get; set; }

        [JsonIgnore, ExcelIgnore]
        public string FileName { get; set; }

        [JsonIgnore, ExcelIgnore, DisplayName("Ana Dosya")]
        public virtual string MainFile { get; set; }

        [JsonIgnore, ExcelIgnore, DisplayName("Diğer Dosyalar")]
        public virtual List<string> OtherFiles { get; set; }
    }
}
