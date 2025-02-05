using N4C.App;
using N4C.Attributes;
using System.Text.Json.Serialization;

namespace APP.Services.Models
{
    public class StoreResponse : Response
    {
        [DisplayName("Adı")]
        public string Name { get; set; }

        public bool Virtual { get; set; }

        [DisplayName("Sanal")]
        [JsonIgnore]
        public string _Virtual { get; set; }

        [DisplayName("Ürün Sayısı")]
        [JsonIgnore]
        public int ProductsCount { get; set; }

        [DisplayName("Ürünler")]
        [JsonIgnore]
        public string Products { get; set; }
    }
}
