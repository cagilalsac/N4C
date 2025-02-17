using APP.Domain;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.App;
using N4C.App.Features;
using N4C.App.Services;
using N4C.Attributes;
using N4C.Domain;
using N4C.Extensions;

namespace APP.Features.Products
{
    public class ProductCommandRequest : CommandRequest
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        public decimal? UnitPrice { get; set; }

        public int? StockAmount { get; set; }

        public DateTime? ExpirationDate { get; set; }

        [Required]
        public int? CategoryId { get; set; }

        public List<int> StoreIds { get; set; }
    }

    public class ProductCommandHandler : Handler<Product, ProductCommandRequest>
    {
        public ProductCommandHandler(IDb db, HttpService httpService, ILogger<Application> logger) : base(db, httpService, logger)
        {
        }

        protected override IQueryable<Product> Data(Action<MapperProfile> mapperProfile = null)
        {
            return base.Data(mapperProfile).Include(p => p.ProductStores);
        }

        public override Result<ProductCommandRequest> Validate(ProductCommandRequest request, ModelStateDictionary modelState = null, string errorMessagesSeperator = "; ")
        {
            if ((request.StockAmount ?? 0) < 0)
                modelState.AddError("Stok miktarı 0 veya pozitif bir sayı olmalıdır", "Stock amount must be 0 or a positive number");
            if ((request.UnitPrice ?? 0) <= 0 || (request.UnitPrice ?? 0) > 100000)
                modelState.AddError("Birim fiyat 0'dan büyük 100000'den küçük olmalıdır", "Unit price must be greater than 0 and less than 100000");
            return base.Validate(request, modelState, errorMessagesSeperator);
        }

        public override Task<Result<ProductCommandRequest>> Update(ProductCommandRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Update(Data(request).ProductStores);
            return base.Update(request, save, cancellationToken);
        }

        public override Task<Result<ProductCommandRequest>> Delete(ProductCommandRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Delete(Data(request).ProductStores);
            return base.Delete(request, save, cancellationToken);
        }
    }
}
