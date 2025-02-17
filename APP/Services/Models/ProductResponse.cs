using N4C.App.Services.Files.Models;
using N4C.Attributes;
using System.Text.Json.Serialization;

namespace APP.Services.Models
{
    public class ProductResponse : FileResponse
    {
        [DisplayName("Adı")]
        public string Name { get; set; }

        [DisplayName("Birim Fiyatı")]
        public decimal? UnitPrice { get; set; }

        [DisplayName("Birim Fiyatı")]
        [JsonIgnore]
        [ExcelIgnore]
        public string UnitPriceS { get; set; }

        [DisplayName("Birim Fiyatı Yazı")]
        [JsonIgnore]
        [ExcelIgnore]
        public string UnitPriceText { get; set; }

        [DisplayName("Stok Miktarı")]
        public int? StockAmount { get; set; }

        [DisplayName("Stok Miktarı")]
        [JsonIgnore]
        [ExcelIgnore]
        public string StockAmountS { get; set; }

        [ExcelIgnore]
        public DateTime? ExpirationDate { get; set; }

        [DisplayName("Son Kullanma Tarihi")]
        [JsonIgnore]
        public string ExpirationDateS { get; set; }

        [ExcelIgnore]
        public int? CategoryId { get; set; }

        [DisplayName("Kategori")]
        [JsonIgnore]
        public string Category { get; set; }

        [ExcelIgnore]
        public List<int> StoreIds { get; set; }

        [DisplayName("Mağazalar")]
        [JsonIgnore]
        public string Stores { get; set; }
    }
}
