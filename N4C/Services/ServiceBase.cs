using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using N4C.Domain;
using N4C.Extensions;
using N4C.Models;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Security.Claims;

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

        protected Result Result(Result previousResult, string tr = default, string en = default) 
            => Result(previousResult.HttpStatusCode, previousResult.Id, tr is null && en is null ? previousResult.Message : Culture == Cultures.TR ? tr : en);

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

        public int GetUserId() => Convert.ToInt32(HttpContextAccessor.HttpContext?.User.Claims?.SingleOrDefault(claim => claim.Type == nameof(Entity.Id))?.Value);

        public T GetSession<T>(string key) where T : class
        {
            return JsonConvert.DeserializeObject<T>(HttpContextAccessor.HttpContext.Session.GetString(key));
        }

        public void CreateSession<T>(string key, T instance) where T : class
        {
            HttpContextAccessor.HttpContext.Session.SetString(key, JsonConvert.SerializeObject(instance));
        }

        public void DeleteSession(string key)
        {
            HttpContextAccessor.HttpContext.Session.Remove(key);
        }

        public string GetCookie(string key)
        {
            return HttpContextAccessor.HttpContext.Request.Cookies[key];
        }

        public void CreateCookie(string key, string value, CookieOptions cookieOptions)
        {
            HttpContextAccessor.HttpContext.Response.Cookies.Append(key, value, cookieOptions);
        }

        public void CreateCookie(string key, string value, bool isHttpOnly = true, int? expirationInMinutes = default)
        {
            var cookieOptions = new CookieOptions()
            {
                Expires = expirationInMinutes.HasValue ?
                    DateTime.SpecifyKind(DateTime.Now.AddMinutes(expirationInMinutes.Value), DateTimeKind.Utc) : DateTimeOffset.MaxValue,
                HttpOnly = isHttpOnly
            };
            CreateCookie(key, value, cookieOptions);
        }

        public void DeleteCookie(string key, bool isHttpOnly = true)
        {
            var cookieOptions = new CookieOptions()
            {
                Expires = DateTime.SpecifyKind(DateTime.Now.AddDays(-1), DateTimeKind.Utc),
                HttpOnly = isHttpOnly
            };
            CreateCookie(key, string.Empty, cookieOptions);
        }

        public async Task SignIn(List<Claim> claims, DateTime? expiration = default, bool isPersistent = true, string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme)
        {
            var identity = new ClaimsIdentity(claims, authenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authenticationProperties = new AuthenticationProperties() { IsPersistent = isPersistent };
            if (expiration.HasValue)
                authenticationProperties.ExpiresUtc = DateTime.SpecifyKind(expiration.Value, DateTimeKind.Utc);
            await HttpContextAccessor.HttpContext.SignInAsync(authenticationScheme, principal, authenticationProperties);
        }

        public async Task SignOut(string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme)
        {
            await HttpContextAccessor.HttpContext.SignOutAsync(authenticationScheme);
        }

        public void GetResponse(byte[] data, string fileName, string contentType)
        {
            if (data is not null && data.Length > 0)
            {
                HttpContextAccessor.HttpContext.Response.Headers.Clear();
                HttpContextAccessor.HttpContext.Response.Clear();
                HttpContextAccessor.HttpContext.Response.ContentType = contentType;
                HttpContextAccessor.HttpContext.Response.Headers.Append("content-length", data.Length.ToString());
                HttpContextAccessor.HttpContext.Response.Headers.Append("content-disposition", "attachment; filename=\"" + fileName + "\"");
                HttpContextAccessor.HttpContext.Response.Body.WriteAsync(data, 0, data.Length);
                HttpContextAccessor.HttpContext.Response.Body.Flush();
            }
        }
    }
}
