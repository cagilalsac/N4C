using APP.Domain;
using Microsoft.EntityFrameworkCore;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Files;
using N4C.Domain;

namespace APP.Features.Products
{
    public class ProductDeleteRequest : DeleteRequest
    {
    }

    public class ProductDeleteHandler : Handler<Product, ProductDeleteRequest>
    {
        public ProductDeleteHandler(IDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
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
