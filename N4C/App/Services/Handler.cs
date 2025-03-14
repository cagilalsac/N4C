using MediatR;
using N4C.App.Services.Files;
using N4C.Domain;

namespace N4C.App.Services
{
    public abstract class Handler<TEntity, TCommandRequest> : Handler<TEntity, TCommandRequest, Response>
        where TEntity : Entity, new() where TCommandRequest : CommandRequest, IRequest<Result<List<Response>>>, new()
    {
        protected Handler(IDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
        {
        }
    }
}
