using APP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C;
using N4C.App;
using N4C.App.Features;
using N4C.App.Services;
using N4C.Attributes;
using N4C.Domain;

namespace APP.Features.Stores
{
    public class StoreCommandRequest : CommandRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Name { get; set; }

        public bool Virtual { get; set; }
    }

    public class StoreCommandHandler : Handler<Store, StoreCommandRequest>
    {
        public StoreCommandHandler(IDb db, HttpService httpService, ILogger<Application> logger) : base(db, httpService, logger)
        {
        }

        protected override IQueryable<Store> Data(Action<MapperProfile> mapperProfile = null)
        {
            return base.Data(mapperProfile).Include(c => c._ProductStores);
        }

        public override Task<Result<StoreCommandRequest>> Delete(StoreCommandRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Delete(Data(request)._ProductStores);
            return base.Delete(request, save, cancellationToken);
        }
    }
}
