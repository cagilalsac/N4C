using APP.Domain;
using Microsoft.EntityFrameworkCore;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Files;

namespace APP.Features.Categories
{
    public class CategoryDeleteRequest : DeleteRequest
    {
    }

    public class CategoryDeleteHandler : Handler<Category, CategoryDeleteRequest>
    {
        public CategoryDeleteHandler(IAppDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
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
