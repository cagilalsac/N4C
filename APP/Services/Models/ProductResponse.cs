using N4C.App.Services.Files.Models;
using N4C.Attributes;

namespace APP.Services.Models
{
    public class ProductResponse : FileResponse
    {
        [DisplayName("Adı")]
        public string Name { get; set; }

        [DisplayName("Birim Fiyatı")]
        public decimal? UnitPrice { get; set; }

        [DisplayName("Stok Miktarı")]
        public int? StockAmount { get; set; }

        [DisplayName("Son Kullanma Tarihi")]
        [ExcelIgnore]
        public DateTime? ExpirationDate { get; set; }

        [ExcelIgnore]
        public int? CategoryId { get; set; }

        [ExcelIgnore]
        public List<int> StoreIds { get; set; }

        [DisplayName("Birim Fiyatı")]
        [ExcelIgnore]
        public string UnitPriceS { get; set; }

        [DisplayName("Birim Fiyatı Yazı")]
        [ExcelIgnore]
        public string UnitPriceText { get; set; }

        [DisplayName("Stok Miktarı")]
        [ExcelIgnore]
        public string StockAmountS { get; set; }

        [DisplayName("Son Kullanma Tarihi")]
        public string ExpirationDateS { get; set; }

        [DisplayName("Kategori")]
        public string Category { get; set; }

        [DisplayName("Mağazalar")]
        [ExcelIgnore]
        public string Stores { get; set; }

        [DisplayName("Mağazalar", "Stores")]
        public string StoresExcel { get; set; }
    }
}
