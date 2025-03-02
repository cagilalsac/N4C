using MediatR;
using Microsoft.Extensions.Logging;
using N4C.App.Services;
using N4C.Domain;

namespace N4C.App.Features
{
    public abstract class Handler<TEntity, TRequest, TResponse> : Service<TEntity, TRequest, TResponse>,
        IRequestHandler<TRequest, Result<List<TResponse>>>
        where TEntity : Entity, new() where TRequest : Request, IRequest<Result<List<TResponse>>>, new() where TResponse : Response, new()
    {
        protected Handler(IDb db, HttpService httpService, ILogger<Service> logger) : base(db, httpService, logger)
        {
            SetCulture(Cultures.EN);
        }

        public virtual async Task<Result<List<TResponse>>> Handle(TRequest request, CancellationToken cancellationToken = default)
        {
            List<TResponse> list = null;
            Result<TRequest> requestResult = Error(request);
            if (request is CreateRequest)
            {
                requestResult = Validate(request, (request as CreateRequest).ModelState);
                if (requestResult.Success)
                    requestResult = await Create(request, true, cancellationToken);
            }
            else if (request is UpdateRequest)
            {
                requestResult = Validate(request, (request as UpdateRequest).ModelState);
                if (requestResult.Success)
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
