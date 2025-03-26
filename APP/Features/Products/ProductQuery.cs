using APP.Domain;
using Microsoft.EntityFrameworkCore;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Files;

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
        public ProductQueryHandler(IAppDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
        {
        }

        protected override IQueryable<Product> Query(Action<QueryConfig> config = null)
        {
            return base.Query(config).Include(p => p.ProductStores);
        }
    }
}
