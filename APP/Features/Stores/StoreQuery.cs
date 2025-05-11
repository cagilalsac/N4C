using APP.Domain;
using Microsoft.EntityFrameworkCore;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Files;
using N4C.Extensions;

namespace APP.Features.Stores
{
    public class StoreQueryRequest : QueryRequest<StoreQueryResponse>
    {
    }

    public class StoreQueryResponse : Response
    {
        public string Name { get; set; }
        public bool Virtual { get; set; }
        public string VirtualS { get; set; }
        public int ProductsCount { get; set; }
        public string Products { get; set; }
    }

    public class StoreQueryHandler : Handler<Store, StoreQueryRequest, StoreQueryResponse>
    {
        public StoreQueryHandler(IAppDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
        {
            Set(c =>
            {
                c.Response
                    .Property(d => d.VirtualS, s => s.Virtual.ToHtml(c.TrueHtml, c.FalseHtml))
                    .Property(d => d.ProductsCount, s => s.ProductStores.Count(ps => !ps.Product.Deleted))
                    .Property(d => d.Products, s => string.Join("<br>", s.ProductStores.Where(ps => !ps.Product.Deleted).Select(ps => ps.Product.Name)));
            });
        }

        protected override IQueryable<Store> Query()
        {
            return base.Query().Include(s => s.ProductStores).ThenInclude(ps => ps.Product).OrderByDescending(s => s.Virtual).ThenBy(s => s.Name);
        }
    }
}
