using APP.Domain;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Files;
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
        public CategoryCreateHandler(IDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
        {
        }
    }
}
