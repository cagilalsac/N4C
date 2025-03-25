using N4C.App;
using N4C.Attributes;

namespace APP.Services.Models
{
    public class StoreResponse : Response
    {
        [DisplayName("Adı")]
        public string Name { get; set; }

        public bool Virtual { get; set; }

        [DisplayName("Sanal")]
        public string VirtualS { get; set; }

        [DisplayName("Ürün Sayısı")]
        public int ProductsCount { get; set; }

        [DisplayName("Ürünler")]
        public string Products { get; set; }
    }
}
