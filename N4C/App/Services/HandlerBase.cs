using MediatR;
using N4C.App.Services.Files;
using N4C.Domain;

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
            Result<TRequest> requestResult = Error(request);
            if (request is CreateRequest)
            {
                requestResult = await Create(request, true, cancellationToken);
            }
            else if (request is UpdateRequest)
            {
                requestResult = await Update(request, true, cancellationToken);
            }
            else if (request is DeleteRequest)
            {
                requestResult = await Delete(request, true, cancellationToken);
            }
            else
            {
                if (request.Id <= 0)
                    return await GetList(cancellationToken);
                var responseResult = await GetItem(request.Id, cancellationToken);
                if (responseResult.Success)
                {
                    list = [responseResult.Data];
                    return Success(list, responseResult);
                }
                return Error(list, responseResult);
            }
            if (requestResult.Success)
            {
                list = [new TResponse() { Id = request.Id }];
                return Success(list, requestResult);
            }
            return Error(list, requestResult);
        }
    }
}
