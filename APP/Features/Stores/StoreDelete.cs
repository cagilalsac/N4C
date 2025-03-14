using APP.Domain;
using Microsoft.EntityFrameworkCore;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Files;
using N4C.Domain;

namespace APP.Features.Stores
{
    public class StoreDeleteRequest : DeleteRequest
    {
    }

    public class StoreDeleteHandler : Handler<Store, StoreDeleteRequest>
    {
        public StoreDeleteHandler(IDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
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
