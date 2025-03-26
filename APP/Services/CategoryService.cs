using APP.Domain;
using APP.Services.Models;
using Microsoft.EntityFrameworkCore;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Files;

namespace APP.Services
{
    public class CategoryService : Service<Category, CategoryRequest, CategoryResponse>
    {
        public CategoryService(IAppDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
        {
            SetTitle("Kategori", "Category");
        }

        protected override IQueryable<Category> Query(Action<QueryConfig> config = default)
        {
            return base.Query().Include(c => c.Products).OrderBy(c => c.Name);
        }

        public override Task<Result<CategoryRequest>> Delete(CategoryRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Validate(Query(request).Products);
            return base.Delete(request, save, cancellationToken);
        }
    }
}
