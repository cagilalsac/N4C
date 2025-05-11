using APP.Domain;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Files;
using N4C.Attributes;

namespace APP.Features.Products
{
    public class ProductCreateRequest : CreateRequest
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

    public class ProductCreateHandler : Handler<Product, ProductCreateRequest>
    {
        public ProductCreateHandler(IAppDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
        {
        }

        protected override Result<ProductCreateRequest> Validate(ProductCreateRequest request)
        {
            if ((request.StockAmount ?? 0) < 0)
                request.Add("Stok miktarı 0 veya pozitif bir sayı olmalıdır", "Stock amount must be 0 or a positive number");
            if ((request.UnitPrice ?? 0) <= 0 || (request.UnitPrice ?? 0) > 100000)
                request.Add("Birim fiyat 0'dan büyük 100000'den küçük olmalıdır", "Unit price must be greater than 0 and less than 100000");
            return base.Validate(request);
        }
    }
}
