using APP.Domain;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Files;
using N4C.Attributes;

namespace APP.Features.Categories
{
    public class CategoryUpdateRequest : UpdateRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }

    public class CategoryUpdateHandler : Handler<Category, CategoryUpdateRequest>
    {
        public CategoryUpdateHandler(IAppDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
        {
        }
    }
}
