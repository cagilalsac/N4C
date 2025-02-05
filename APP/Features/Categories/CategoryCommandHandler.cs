using APP.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C;
using N4C.App;
using N4C.App.Features;
using N4C.App.Services;
using N4C.Attributes;
using N4C.Domain;

namespace APP.Features.Categories
{
    public class CategoryCommandRequest : CommandRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }

    public class CategoryCommandHandler : Handler<Category, CategoryCommandRequest>
    {
        public CategoryCommandHandler(IDb db, HttpService httpService, ILogger<Application> logger) : base(db, httpService, logger)
        {
        }

        protected override IQueryable<Category> Data(Action<MapperProfile> mapperProfile = null)
        {
            return base.Data(mapperProfile).Include(c => c._Products);
        }

        public override Task<Result<CategoryCommandRequest>> Delete(CategoryCommandRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Validate(Data(request)._Products);
            return base.Delete(request, save, cancellationToken);
        }
    }
}
