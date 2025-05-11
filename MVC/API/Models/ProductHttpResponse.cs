#nullable disable

namespace MVC.API.Models
{
    public class ProductHttpResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? UnitPrice { get; set; }
        public int? StockAmount { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? CategoryId { get; set; }
        public List<int> StoreIds { get; set; }
    }
}
