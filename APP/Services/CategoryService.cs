using APP.Domain;
using APP.Services.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.App;
using N4C.App.Services;
using N4C.Domain;

namespace APP.Services
{
    public class CategoryService : Service<Category, CategoryRequest, CategoryResponse>
    {
        public CategoryService(IDb db, HttpService httpService, ILogger<Application> logger) : base(db, httpService, logger)
        {
            SetTitle("Kategori", "Category");
        }

        protected override IQueryable<Category> Data(Action<MapperProfile> config = default)
        {
            return base.Data().Include(c => c.Products).OrderBy(c => c.Name);
        }

        public override Task<Result<CategoryRequest>> Delete(CategoryRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Validate(Data(request).Products);
            return base.Delete(request, save, cancellationToken);
        }
    }
}
