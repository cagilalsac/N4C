using MediatR;
using Microsoft.Extensions.Logging;
using N4C.App.Services;
using N4C.Domain;

namespace N4C.App.Features
{
    public abstract class Handler<TEntity, TCommandRequest> : Handler<TEntity, TCommandRequest, Response>
        where TEntity : Entity, new() where TCommandRequest : CommandRequest, IRequest<Result<List<Response>>>, new()
    {
        protected Handler(IDb db, HttpService httpService, ILogger<Service> logger) : base(db, httpService, logger)
        {
        }
    }
}
