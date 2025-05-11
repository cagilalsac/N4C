using APP.Domain;
using Microsoft.EntityFrameworkCore;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Files;
using N4C.Extensions;
using System.Globalization;

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
        public string UnitPriceS { get; set; }
        public string UnitPriceText { get; set; }
        public string StockAmountS { get; set; }
        public string ExpirationDateS { get; set; }
        public string Category { get; set; }
        public string Stores { get; set; }
        public string StoresExcel { get; set; }
    }

    public class ProductQueryHandler : Handler<Product, ProductQueryRequest, ProductQueryResponse>
    {
        public ProductQueryHandler(IAppDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
        {
            Set(c =>
            {
                c.Response
                    .Property(d => d.UnitPriceS, s => (s.UnitPrice ?? 0).ToString("C2", new CultureInfo("tr-TR")))
                    .Property(d => d.StockAmountS, s =>
                        s.StockAmount.HasValue ?
                        "<span class=" +
                            (s.StockAmount.Value < 10 ? "'badge bg-danger'>"
                            : s.StockAmount.Value < 100 ? "'badge bg-warning'>"
                            : "'badge bg-success'>") + s.StockAmount.Value + "</span>"
                        : string.Empty)
                    .Property(d => d.ExpirationDateS, s => s.ExpirationDate.HasValue ? s.ExpirationDate.Value.ToShortDateString() : "")
                    .Property(d => d.Category, s => s.Category.Name)
                    .Property(d => d.Stores, s => string.Join("<br>", s.ProductStores.OrderBy(ps => ps.Store.Name).Select(ps => ps.Store.Name)))
                    .Property(d => d.UnitPriceText, s => (s.UnitPrice ?? 0).ToMoneyString(Cultures.TR, false))
                    .Property(d => d.StoresExcel, s => string.Join(", ", s.ProductStores.OrderBy(ps => ps.Store.Name).Select(ps => ps.Store.Name)));
                c.SplitQuery = true;
            });
        }

        protected override IQueryable<Product> Query()
        {
            return base.Query().Include(p => p.ProductStores).ThenInclude(ps => ps.Store).Include(p => p.Category).OrderBy(p => p.Name);
        }
    }
}
