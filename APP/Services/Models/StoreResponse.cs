using N4C.App;
using N4C.Attributes;
using System.Text.Json.Serialization;

namespace APP.Services.Models
{
    public class StoreResponse : Response
    {
        [DisplayName("Adı")]
        public string Name { get; set; }

        [ExcelIgnore]
        public bool Virtual { get; set; }

        [DisplayName("Sanal")]
        [JsonIgnore]
        [ExcelIgnore]
        public string _Virtual { get; set; }

        [JsonIgnore]
        public string _IsVirtual { get; set; }

        [DisplayName("Ürün Sayısı")]
        [JsonIgnore]
        public int ProductsCount { get; set; }

        [DisplayName("Ürünler")]
        [JsonIgnore]
        [ExcelIgnore]
        public string Products { get; set; }
    }
}
