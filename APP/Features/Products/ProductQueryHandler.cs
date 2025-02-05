using APP.Domain;
using Microsoft.Extensions.Logging;
using N4C.App;
using N4C.App.Features;
using N4C.App.Services;
using N4C.Domain;

namespace APP.Features.Products
{
    public class ProductQueryRequest : QueryRequest<ProductQueryResponse>
    {
    }

    public class ProductQueryResponse : Response
    {
        public string Name { get; set; }
        public decimal? UnitPrice { get; set; }
        public int? StockAmount { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? CategoryId { get; set; }
        public List<int> StoreIds { get; set; }
    }

    public class ProductQueryHandler : Handler<Product, ProductQueryRequest, ProductQueryResponse>
    {
        public ProductQueryHandler(IDb db, HttpService httpService, ILogger<Application> logger) : base(db, httpService, logger)
        {
        }
    }
}
