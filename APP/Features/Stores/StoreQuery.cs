using APP.Domain;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Files;

namespace APP.Features.Stores
{
    public class StoreQueryRequest : QueryRequest<StoreQueryResponse>
    {
    }

    public class StoreQueryResponse : Response
    {
        public string Name { get; set; }
        public bool Virtual { get; set; }
    }

    public class StoreQueryHandler : Handler<Store, StoreQueryRequest, StoreQueryResponse>
    {
        public StoreQueryHandler(IAppDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
        {
        }
    }
}
