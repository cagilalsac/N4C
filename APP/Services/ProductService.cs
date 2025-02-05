using APP.Domain;
using APP.Services.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C;
using N4C.App;
using N4C.App.Services;
using N4C.Domain;
using N4C.Extensions;
using System.Globalization;

namespace APP.Services
{
    public class ProductService : Service<Product, ProductRequest, ProductResponse>
    {
        public ProductService(IDb db, HttpService httpService, ILogger<Application> logger) : base(db, httpService, logger)
        {
            SetPageOrder(true);
            SetOrderExpressions(p => p.Name, p => p.UnitPrice, p => p.StockAmount, p => p.ExpirationDate);
            SetTitle("Ürün", "Product");
        }

        protected override IQueryable<Product> Data(Action<MapperProfile> mapperProfile = null)
        {
            return base.Data(p =>
            {
                p.CreateMap<Product, ProductResponse>()
                    .ForMember(d => d._UnitPrice, s => (s.UnitPrice ?? 0).ToString("C2", new CultureInfo("tr-TR")))
                    .ForMember(d => d._StockAmount, s =>
                        s.StockAmount.HasValue ?
                        "<span class=" +
                            (s.StockAmount.Value < 10 ? "'badge bg-danger'>"
                            : s.StockAmount.Value < 100 ? "'badge bg-warning'>"
                            : "'badge bg-success'>") + s.StockAmount.Value + "</span>"
                        : string.Empty)
                    .ForMember(d => d._ExpirationDate, s => s.ExpirationDate.HasValue ? s.ExpirationDate.Value.ToShortDateString() : "")
                    .ForMember(d => d.Category, s => s._Category.Name)
                    .ForMember(d => d.Stores, s => string.Join("<br>", s._ProductStores.OrderBy(ps => ps._Store.Name).Select(ps => ps._Store.Name)))
                    .ForMember(d => d._UnitPriceText, s => (s.UnitPrice ?? 0).ToMoneyString(Cultures.TR, false));
            }).Include(p => p._ProductStores).ThenInclude(ps => ps._Store).Include(p => p._Category).OrderBy(p => p.Name);
        }

        public override Result<ProductRequest> Validate(ProductRequest request, ModelStateDictionary modelState = null, string errorMessagesSeperator = "; ")
        {
            if ((request.StockAmount ?? 0) < 0)
                modelState.AddError("Stok miktarı 0 veya pozitif bir sayı olmalıdır", "Stock amount must be 0 or a positive number");
            if ((request.UnitPrice ?? 0) <= 0 || (request.UnitPrice ?? 0) > 100000)
                modelState.AddError("Birim fiyat 0'dan büyük 100000'den küçük olmalıdır", "Unit price must be greater than 0 and less than 100000");
            return base.Validate(request, modelState, errorMessagesSeperator);
        }

        public override Task<Result<ProductRequest>> Update(ProductRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Update(Data(request)._ProductStores);
            return base.Update(request, save, cancellationToken);
        }

        public override Task<Result<ProductRequest>> Delete(ProductRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Delete(Data(request)._ProductStores);
            return base.Delete(request, save, cancellationToken);
        }
    }
}
