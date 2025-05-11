using APP.Domain;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Files;

namespace APP.Features.Categories
{
    public class CategoryQueryRequest : QueryRequest<CategoryQueryResponse>
    {
    }

    public class CategoryQueryResponse : Response
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CategoryQueryHandler : Handler<Category, CategoryQueryRequest, CategoryQueryResponse>
    {
        public CategoryQueryHandler(IAppDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
        {
        }
    }
}
