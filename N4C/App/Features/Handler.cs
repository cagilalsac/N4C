using MediatR;
using Microsoft.Extensions.Logging;
using N4C.App.Services;
using N4C.Domain;

namespace N4C.App.Features
{
    public abstract class Handler<TEntity, TRequest, TResponse> : Application<TEntity, TRequest, TResponse>, 
        IRequestHandler<TRequest, Result<List<TResponse>>>
        where TEntity : Entity, new() where TRequest : Request<TResponse>, IRequest<Result<List<TResponse>>>, new() where TResponse : Response, new()
    {
        protected Handler(IDb db, HttpService httpService, ILogger<Application> logger) : base(db, httpService, logger)
        {
            SetCulture(Cultures.EN);
        }

        public virtual async Task<Result<List<TResponse>>> Handle(TRequest request, CancellationToken cancellationToken = default)
        {
            List<TResponse> list = null;
            Result<TRequest> requestResult = Error(request);
            switch (request.Command)
            {
                case Commands.Create:
                    requestResult = Validate(request, (request as CommandRequest).ModelState);
                    if (requestResult.Success)
                        requestResult = await Create(request, true, cancellationToken);
                    break;
                case Commands.Update:
                    requestResult = Validate(request, (request as CommandRequest).ModelState);
                    if (requestResult.Success)
                        requestResult = await Update(request, true, cancellationToken);
                    break;
                case Commands.Delete:
                    requestResult = await Delete(request, true, cancellationToken);
                    break;
                default:
                    if (request.Id <= 0)
                        return await GetList(cancellationToken);
                    var responseResult = await GetItem(request.Id, cancellationToken);
                    if (responseResult.Success)
                    {
                        list = [responseResult.Data];
                        return Success(list, responseResult.Message);
                    }
                    return Error(list, responseResult.HttpStatusCode);
            }
            if (requestResult.Success)
            {
                list = [new TResponse() { Id = request.Id }];
                return Success(list, requestResult.Message);
            }
            return Error(list, requestResult.HttpStatusCode, requestResult.Message);
        }
    }

    public abstract class Handler<TEntity, TCommandRequest> : Handler<TEntity, TCommandRequest, Response>
        where TEntity : Entity, new() where TCommandRequest : CommandRequest, IRequest<Result<List<Response>>>, new()
    {
        protected Handler(IDb db, HttpService httpService, ILogger<Application> logger) : base(db, httpService, logger)
        {
        }
    }
}
