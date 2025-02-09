using APP.Domain;
using APP.Services.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C;
using N4C.App;
using N4C.App.Services;
using N4C.Domain;
using N4C.Extensions;

namespace APP.Services
{
    public class StoreService : Service<Store, StoreRequest, StoreResponse>
    {
        public StoreService(IDb db, HttpService httpService, ILogger<Application> logger) : base(db, httpService, logger)
        {
            SetTitle("Mağaza", "Store");
        }

        protected override IQueryable<Store> Data(Action<MapperProfile> mapperProfile = null)
        {
            return base.Data(p =>
            {
                p.CreateMap<Store, StoreResponse>()
                    .ForMember(d => d._Virtual, s => s.Virtual.ToHtml(TrueHtml, FalseHtml))
                    .ForMember(d => d._IsVirtual, s => s.Virtual ? (Culture == Cultures.TR ? "Evet" : "Yes") : (Culture == Cultures.TR ? "Hayır" : "No"))
                    .ForMember(d => d.ProductsCount, s => s._ProductStores.Count(ps => !ps._Product.Deleted))
                    .ForMember(d => d.Products, s => string.Join("<br>", s._ProductStores.Where(ps => !ps._Product.Deleted).Select(ps => ps._Product.Name)));
            }).Include(s => s._ProductStores).ThenInclude(ps => ps._Product).OrderByDescending(s => s.Virtual).ThenBy(s => s.Name);
        }

        public override Task<Result<StoreRequest>> Delete(StoreRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Delete(Data(request)._ProductStores);
            return base.Delete(request, save, cancellationToken);
        }
    }
}
