using APP.Domain;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.App;
using N4C.App.Features;
using N4C.App.Services;
using N4C.Attributes;
using N4C.Domain;

namespace APP.Features.Products
{
    public class ProductUpdateRequest : UpdateRequest
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

    public class ProductUpdateHandler : Handler<Product, ProductUpdateRequest>
    {
        public ProductUpdateHandler(IDb db, HttpService httpService, ILogger<Service> logger) : base(db, httpService, logger)
        {
        }

        protected override IQueryable<Product> Query(Action<MapperProfile> mapperProfile = null)
        {
            return base.Query(mapperProfile).Include(p => p.ProductStores);
        }

        public override Result<ProductUpdateRequest> Validate(ProductUpdateRequest request, ModelStateDictionary modelState = null, string errorMessagesSeperator = "; ")
        {
            if ((request.StockAmount ?? 0) < 0)
                AddError("Stok miktarı 0 veya pozitif bir sayı olmalıdır", "Stock amount must be 0 or a positive number");
            if ((request.UnitPrice ?? 0) <= 0 || (request.UnitPrice ?? 0) > 100000)
                AddError("Birim fiyat 0'dan büyük 100000'den küçük olmalıdır", "Unit price must be greater than 0 and less than 100000");
            return base.Validate(request, modelState, errorMessagesSeperator);
        }

        public override Task<Result<ProductUpdateRequest>> Update(ProductUpdateRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Update(Query(request).ProductStores);
            return base.Update(request, save, cancellationToken);
        }
    }
}
