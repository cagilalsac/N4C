using APP.Domain;
using APP.Services.Models;
using Microsoft.EntityFrameworkCore;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Files;
using N4C.Extensions;
using System.Globalization;

namespace APP.Services
{
    public class ProductService : Service<Product, ProductRequest, ProductResponse>
    {
        public ProductService(IAppDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
        {
            SetPageOrder(true);
            SetOrderExpressions(p => p.Name, p => p.UnitPrice, p => p.StockAmount, p => p.ExpirationDate);
            SetTitle("Ürün");
        }

        protected override IQueryable<Product> Query(Action<MapperProfile> mapperProfile = null)
        {
            return base.Query(p =>
            {
                p.Map<Product, ProductResponse>()
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
            }).Include(p => p.ProductStores).ThenInclude(ps => ps.Store).Include(p => p.Category).OrderBy(p => p.Name);
        }

        protected override Result<ProductRequest> Validate(ProductRequest request)
        {
            if ((request.StockAmount ?? 0) < 0)
                request.Add("Stok miktarı 0 veya pozitif bir sayı olmalıdır", "Stock amount must be 0 or a positive number");
            if ((request.UnitPrice ?? 0) <= 0 || (request.UnitPrice ?? 0) > 100000)
                request.Add("Birim fiyat 0'dan büyük 100000'den küçük olmalıdır", "Unit price must be greater than 0 and less than 100000");
            return base.Validate(request);
        }

        public override Task<Result<ProductRequest>> Update(ProductRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Update(Query(request).ProductStores);
            return base.Update(request, save, cancellationToken);
        }

        public override Task<Result<ProductRequest>> Delete(ProductRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Delete(Query(request).ProductStores);
            return base.Delete(request, save, cancellationToken);
        }
    }
}
