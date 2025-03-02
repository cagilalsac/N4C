using APP.Domain;
using APP.Services.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.App;
using N4C.App.Services;
using N4C.Domain;
using N4C.Extensions;

namespace APP.Services
{
    public class StoreService : Service<Store, StoreRequest, StoreResponse>
    {
        public StoreService(IDb db, HttpService httpService, ILogger<Service> logger) : base(db, httpService, logger)
        {
            SetTitle("Mağaza", "Store");
        }

        protected override IQueryable<Store> Query(Action<MapperProfile> mapperProfile = null)
        {
            return base.Query(p =>
            {
                p.Map<Store, StoreResponse>()
                    .Property(d => d.VirtualS, s => s.Virtual.ToHtml(TrueHtml, FalseHtml))
                    .Property(d => d.ProductsCount, s => s.ProductStores.Count(ps => !ps.Product.Deleted))
                    .Property(d => d.Products, s => string.Join("<br>", s.ProductStores.Where(ps => !ps.Product.Deleted).Select(ps => ps.Product.Name)));
            }).Include(s => s.ProductStores).ThenInclude(ps => ps.Product).OrderByDescending(s => s.Virtual).ThenBy(s => s.Name);
        }

        public override Task<Result<StoreRequest>> Delete(StoreRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Delete(Query(request).ProductStores);
            return base.Delete(request, save, cancellationToken);
        }
    }
}
