using N4C.App.Services.Files.Models;
using N4C.Attributes;

namespace APP.Services.Models
{
    public class ProductRequest : FileRequest
    {
        [Required]
        [StringLength(150)]
        [DisplayName("Adı")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Birim Fiyatı")]
        public decimal? UnitPrice { get; set; }

        [DisplayName("Stok Miktarı")]
        public int? StockAmount { get; set; }

        [DisplayName("Son Kullanma Tarihi")]
        public DateTime? ExpirationDate { get; set; }

        [Required]
        [DisplayName("Kategori")]
        public int? CategoryId { get; set; }

        [DisplayName("Mağazalar")]
        public List<int> StoreIds { get; set; }
    }
}
