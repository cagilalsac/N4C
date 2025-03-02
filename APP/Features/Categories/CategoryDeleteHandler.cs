using APP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.App;
using N4C.App.Features;
using N4C.App.Services;
using N4C.Domain;

namespace APP.Features.Categories
{
    public class CategoryDeleteRequest : DeleteRequest
    {
    }

    public class CategoryDeleteHandler : Handler<Category, CategoryDeleteRequest>
    {
        public CategoryDeleteHandler(IDb db, HttpService httpService, ILogger<Service> logger) : base(db, httpService, logger)
        {
        }

        protected override IQueryable<Category> Query(Action<MapperProfile> mapperProfile = null)
        {
            return base.Query(mapperProfile).Include(c => c.Products);
        }

        public override Task<Result<CategoryDeleteRequest>> Delete(CategoryDeleteRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Validate(Query(request).Products);
            return base.Delete(request, save, cancellationToken);
        }
    }
}
