using APP.Domain;
using Microsoft.Extensions.Logging;
using N4C.App;
using N4C.App.Features;
using N4C.App.Services;
using N4C.Domain;

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
        public StoreQueryHandler(IDb db, HttpService httpService, ILogger<Application> logger) : base(db, httpService, logger)
        {
        }
    }
}
