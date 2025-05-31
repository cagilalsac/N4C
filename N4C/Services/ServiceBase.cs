using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using N4C.Domain;
using N4C.Extensions;
using N4C.Models;
using System.Globalization;
using System.Net;

namespace N4C.Services
{
    public class Service
    {
        private Config Config { get; set; } = new Config();

        public string Culture => Config.Culture;

        private bool ModelStateErrors { get; set; } = true;

        private IHttpContextAccessor HttpContextAccessor { get; }
        private ILogger<Service> Logger { get; }

        public Service(IHttpContextAccessor httpContextAccessor, ILogger<Service> logger)
        {
            HttpContextAccessor = httpContextAccessor;
            Logger = logger;
        }

        public void Set(string culture, string titleTR, string titleEN)
        {
            Config.SetCulture(culture);
            Config.SetTitle(titleTR, titleEN);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Culture);
        }

        public void SetModelStateErrors(bool modelStateErrors) => ModelStateErrors = modelStateErrors;

        protected Result Result(HttpStatusCode httpStatusCode, int? id = default, string message = default)
        {
            return new Result(httpStatusCode, message, Culture, Config.Title, id);
        }

        protected Result<TData> Result<TData>(HttpStatusCode httpStatusCode, TData data, string message = default) where TData : class, new()
        {
            return new Result<TData>(httpStatusCode, data, message, Culture, Config.Title);
        }

        protected Result Result(Result previousResult) => Result(previousResult.HttpStatusCode, previousResult.Id, previousResult.Message);

        public virtual Result NotFound(int? id = default) => Result(HttpStatusCode.NotFound, id, Config.NotFound);

        public virtual Result Found(IEnumerable<int> ids) 
            => ids.Count() > 1 ? Result(HttpStatusCode.OK, null, $"{ids.Count()} {Config.Found}") :
                ids.Count() == 1 ? Result(HttpStatusCode.OK, ids.First()) :
                NotFound();

        public virtual Result Error(string tr = default, string en = default, int? id = default)
            => Result(HttpStatusCode.BadRequest, id, $"{Config.Error} {(Culture == Cultures.TR ? tr : en)}".TrimEnd());

        public virtual Result Error(Exception exception, int? id = default)
            => Result(HttpStatusCode.InternalServerError, id, Config.Exception);

        public virtual Result Created(int? id = default) => Result(HttpStatusCode.Created, id, Config.Created);
        public virtual Result Updated(int? id = default) => Result(HttpStatusCode.NoContent, id, Config.Updated);
        public virtual Result Deleted(int? id = default) => Result(HttpStatusCode.NoContent, id, Config.Deleted);
        public virtual Result Unauthorized(int? id = default) => Result(HttpStatusCode.Unauthorized, id, Config.Unauthorized);
        public virtual Result Success(string tr = default, string en = default, int? id = default) => Result(HttpStatusCode.OK, id, Culture == Cultures.TR ? tr : en);

        public virtual Result<List<TData>> Found<TData>(List<TData> list) where TData : Data, new()
            => list.Count > 0 ? Result(HttpStatusCode.OK, list, $"{list.Count} {Config.Found}") : 
                Result(HttpStatusCode.NotFound, list, Config.NotFound);

        public virtual Result<TData> NotFound<TData>(TData item) where TData : Data, new()
            => Result(HttpStatusCode.NotFound, item, Config.NotFound);

        public virtual Result<TData> Found<TData>(TData item) where TData : Data, new()
            => item is not null ? Result(HttpStatusCode.OK, item) : NotFound(item);

        public virtual Result<TData> Error<TData>(TData item, string tr = default, string en = default) where TData : Data, new()
            => Result(HttpStatusCode.BadRequest, item, $"{Config.Error} {(Culture == Cultures.TR ? tr : en)}".TrimEnd());

        public virtual Result<List<TData>> Error<TData>(List<TData> list, Exception exception) where TData : Data, new()
        {
            LogError("ServiceException: " + exception.Message);
            return Result(HttpStatusCode.InternalServerError, list, Config.Exception);
        }

        public virtual Result<TData> Error<TData>(TData item, Exception exception) where TData : Data, new()
        {
            LogError("ServiceException: Id = " + item.Id + ": " + exception.Message);
            return Result(HttpStatusCode.InternalServerError, item, Config.Exception);
        }

        public virtual Result<TData> Created<TData>(TData item) where TData : Data, new()
            => Result(HttpStatusCode.Created, item, Config.Created);

        public virtual Result<TData> Updated<TData>(TData item) where TData : Data, new()
            => Result(HttpStatusCode.NoContent, item, Config.Updated);

        public virtual Result<TData> Deleted<TData>(TData item) where TData : Data, new()
            => Result(HttpStatusCode.NoContent, item, Config.Deleted);

        public virtual Result<TData> RelationsFound<TData>(TData item) where TData : Data, new()
            => Result(HttpStatusCode.BadRequest, item, $"{Config.Error} {Config.RelationsFound}");

        public virtual Result<TData> Validated<TData>(TData item, string modelStateErrors, string uniquePropertyError = default) where TData : Data, new()
        {
            var error = modelStateErrors.Length > 0 || (uniquePropertyError ?? string.Empty).Length > 0;
            var errors = Config.Error;
            if (ModelStateErrors && modelStateErrors.Length > 0)
                errors += ";" + modelStateErrors;
            errors += ";" + uniquePropertyError;
            return error ? Result(HttpStatusCode.BadRequest, item, errors.Trim(';')) : Result(HttpStatusCode.OK, item);
        }

        public Result Validate(ModelStateDictionary modelState)
        {
            var errors = modelState.GetErrors(Culture);
            if (errors.Count > 0)
                return Result(HttpStatusCode.BadRequest, null, string.Join(";", errors));
            return Result(HttpStatusCode.OK);
        }

        public void LogError(string message) => Logger.LogError(message);

        public string GetUserName() => HttpContextAccessor.HttpContext?.User.Identity?.Name;
    }
}
