using APP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.App;
using N4C.App.Features;
using N4C.App.Services;
using N4C.Domain;

namespace APP.Features.Products
{
    public class ProductDeleteRequest : DeleteRequest
    {
    }

    public class ProductDeleteHandler : Handler<Product, ProductDeleteRequest>
    {
        public ProductDeleteHandler(IDb db, HttpService httpService, ILogger<Service> logger) : base(db, httpService, logger)
        {
        }

        protected override IQueryable<Product> Query(Action<MapperProfile> mapperProfile = null)
        {
            return base.Query(mapperProfile).Include(p => p.ProductStores);
        }

        public override Task<Result<ProductDeleteRequest>> Delete(ProductDeleteRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Delete(Query(request).ProductStores);
            return base.Delete(request, save, cancellationToken);
        }
    }
}
