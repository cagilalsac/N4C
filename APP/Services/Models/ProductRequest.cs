using Microsoft.AspNetCore.Http;
using N4C.App;
using N4C.Attributes;

namespace APP.Services.Models
{
    public class ProductRequest : Request, IFileRequest
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

        [DisplayName("Ana Dosya")]
        public IFormFile MainFormFile { get; set; }

        [DisplayName("Diğer Dosyalar")]
        public List<IFormFile> OtherFormFiles { get; set; }

        public string MainFile { get; set; }
    }
}
