using APP.Domain;
using Microsoft.Extensions.Logging;
using N4C.App;
using N4C.App.Features;
using N4C.App.Services;
using N4C.Attributes;
using N4C.Domain;

namespace APP.Features.Categories
{
    public class CategoryCreateRequest : CreateRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }

    public class CategoryCreateHandler : Handler<Category, CategoryCreateRequest>
    {
        public CategoryCreateHandler(IDb db, HttpService httpService, ILogger<Service> logger) : base(db, httpService, logger)
        {
        }
    }
}
