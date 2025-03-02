using APP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.App;
using N4C.App.Features;
using N4C.App.Services;
using N4C.Domain;

namespace APP.Features.Stores
{
    public class StoreDeleteRequest : DeleteRequest
    {
    }

    public class StoreDeleteHandler : Handler<Store, StoreDeleteRequest>
    {
        public StoreDeleteHandler(IDb db, HttpService httpService, ILogger<Service> logger) : base(db, httpService, logger)
        {
        }

        protected override IQueryable<Store> Query(Action<MapperProfile> mapperProfile = null)
        {
            return base.Query(mapperProfile).Include(c => c.ProductStores);
        }

        public override Task<Result<StoreDeleteRequest>> Delete(StoreDeleteRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Delete(Query(request).ProductStores);
            return base.Delete(request, save, cancellationToken);
        }
    }
}
