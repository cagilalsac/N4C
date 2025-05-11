using MediatR;
using N4C.App.Services.Files;
using N4C.Domain;
using System.Net;

namespace N4C.App.Services
{
    public abstract class Handler<TEntity, TRequest, TResponse> : Service<TEntity, TRequest, TResponse>,
        IRequestHandler<TRequest, Result<List<TResponse>>>
        where TEntity : Entity, new() where TRequest : Request, IRequest<Result<List<TResponse>>>, new() where TResponse : Response, new()
    {
        protected Handler(IDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
        {
        }

        public virtual async Task<Result<List<TResponse>>> Handle(TRequest request, CancellationToken cancellationToken = default)
        {
            List<TResponse> list = null;
            Result<TRequest> requestResult;
            if (request is CreateRequest)
            {
                requestResult = await Create(request, true, cancellationToken);
                if (requestResult.Success)
                {
                    list = [new TResponse() { Id = request.Id }];
                    return Success(list, requestResult);
                }
                return Error(list, requestResult);
            }
            if (request is UpdateRequest)
            {
                requestResult = await Update(request, true, cancellationToken);
                if (requestResult.Success)
                {
                    list = [new TResponse() { Id = request.Id }];
                    return Success(list, requestResult);
                }
                return Error(list, requestResult);
            }
            if (request is DeleteRequest)
            {
                requestResult = await Delete(request, true, cancellationToken);
                if (requestResult.Success)
                {
                    list = [new TResponse() { Id = request.Id }];
                    return Success(list, requestResult);
                }
                return Error(list, requestResult);
            }
            if (request.Id > 0)
                return await Get(entity => entity.Id == request.Id, cancellationToken);
            if (request.Id == 0)
                return await Get(cancellationToken);
            return Error(list, HttpStatusCode.NotFound);
        }
    }
}
