using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using N4C.Domain;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

namespace N4C.App.Services
{
    public class HttpService : Service
    {
        protected IHttpContextAccessor HttpContextAccessor { get; }
        protected IHttpClientFactory HttpClientFactory { get; }

        public HttpService(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, LogService logService) : base(logService)
        {
            HttpContextAccessor = httpContextAccessor;
            HttpClientFactory = httpClientFactory;
            SetCulture(GetCookie("Culture") ?? Settings.Culture);
        }

        public string GetUserName()
        {
            return HttpContextAccessor.HttpContext?.User.Identity?.Name;
        }

        public int GetUserId()
        {
            return Convert.ToInt32(HttpContextAccessor.HttpContext?.User.Claims?.SingleOrDefault(claim => claim.Type == nameof(Entity.Id))?.Value);
        }

        public T GetSession<T>(string key) where T : class
        {
            return JsonConvert.DeserializeObject<T>(HttpContextAccessor.HttpContext.Session.GetString(key));
        }

        public void SetSession<T>(string key, T instance) where T : class
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

        public void SetCookie(string key, string value, CookieOptions cookieOptions)
        {
            HttpContextAccessor.HttpContext.Response.Cookies.Append(key, value, cookieOptions);
        }

        public void SetCookie(string key, string value, bool isHttpOnly = true, int? expirationInDays = default)
        {
            var cookieOptions = new CookieOptions()
            {
                Expires = expirationInDays.HasValue ?
                    DateTime.SpecifyKind(DateTime.Now.AddDays(expirationInDays.Value), DateTimeKind.Utc) : DateTimeOffset.MaxValue,
                HttpOnly = isHttpOnly
            };
            SetCookie(key, value, cookieOptions);
        }

        public void DeleteCookie(string key, bool isHttpOnly = true)
        {
            var cookieOptions = new CookieOptions()
            {
                Expires = DateTime.SpecifyKind(DateTime.Now.AddDays(-1), DateTimeKind.Utc),
                HttpOnly = isHttpOnly
            };
            SetCookie(key, string.Empty, cookieOptions);
        }

        public async Task SignInAsync(List<Claim> claims, DateTime? expiration = default, bool isPersistent = true, string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme)
        {
            var identity = new ClaimsIdentity(claims, authenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authenticationProperties = new AuthenticationProperties() { IsPersistent = isPersistent };
            if (expiration.HasValue)
                authenticationProperties.ExpiresUtc = DateTime.SpecifyKind(expiration.Value, DateTimeKind.Utc);
            await HttpContextAccessor.HttpContext.SignInAsync(authenticationScheme, principal, authenticationProperties);
        }

        public async Task SignOutAsync(string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme)
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

        public string GetJwt(List<Claim> claims, DateTime? expiration = default)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.JwtSecurityKey));
            var signingCredentials = new SigningCredentials(securityKey, Settings.JwtSecurityAlgorithm);
            var jwtSecurityToken = new JwtSecurityToken(Settings.JwtIssuer, Settings.JwtAudience, claims, DateTime.Now, expiration, signingCredentials);
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            return jwtSecurityTokenHandler.WriteToken(jwtSecurityToken);
        }

        protected string GetToken(string bearerToken)
        {
            var token = bearerToken;
            if (string.IsNullOrWhiteSpace(token))
                token = HttpContextAccessor.HttpContext?.Request?.Headers?.Authorization.FirstOrDefault() ?? GetCookie("JWT") ?? string.Empty;
            if (token.StartsWith(JwtBearerDefaults.AuthenticationScheme))
                token = token.Remove(0, JwtBearerDefaults.AuthenticationScheme.Length).TrimStart();
            return token;
        }

        protected HttpClient CreateHttpClient(string bearerToken = default)
        {
            var httpClient = HttpClientFactory.CreateClient();
            var token = GetToken(bearerToken);
            if (!string.IsNullOrWhiteSpace(token))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
            return httpClient;
        }

        public virtual async Task<Result<List<TResponse>>> GetList<TResponse>(string uri, string bearerToken = default, CancellationToken cancellationToken = default)
            where TResponse : class
        {
            List<TResponse> list = null;
            try
            {
                var httpResponseMessage = await CreateHttpClient(bearerToken).GetAsync(uri, cancellationToken);
                var httpResponse = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                if (httpResponse.Contains(typeof(Result).ToString()))
                    return JsonConvert.DeserializeObject<Result<List<TResponse>>>(httpResponse);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    list = JsonConvert.DeserializeObject<List<TResponse>>(httpResponse);
                    if (list is not null && list.Any())
                        return Success(list, $"{list.Count} {Found}");
                    return Error(list, HttpStatusCode.NotFound);
                }
                return Error(list, httpResponse, httpResponseMessage.StatusCode);
            }
            catch (Exception exception)
            {
                LogService.LogError($"HttpServiceException: {GetType().Name}.GetList(Uri = {uri}): {exception.Message}");
                return Error(list, HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task<Result<TResponse>> GetItem<TResponse>(string uri, int id, string bearerToken = default, CancellationToken cancellationToken = default)
            where TResponse : class
        {
            TResponse item = null;
            try
            {
                var httpResponseMessage = await CreateHttpClient(bearerToken).GetAsync($"{uri}/{id}", cancellationToken);
                var httpResponse = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                if (httpResponse.Contains(typeof(Result).ToString()))
                    return JsonConvert.DeserializeObject<Result<TResponse>>(httpResponse);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    item = JsonConvert.DeserializeObject<TResponse>(httpResponse);
                    if (item is not null)
                        return Success(item, $"1 {Found}", HttpStatusCode.PartialContent);
                    return Error(item, HttpStatusCode.NotFound);
                }
                return Error(item, httpResponse, httpResponseMessage.StatusCode);
            }
            catch (Exception exception)
            {
                LogService.LogError($"HttpServiceException: {GetType().Name}.GetItem(Uri = {uri}, Id = {id}): {exception.Message}");
                return Error(item, HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task<Result> Create<TRequest>(string uri, TRequest request, string bearerToken = default, CancellationToken cancellationToken = default)
            where TRequest : class
        {
            try
            {
                var httpResponseMessage = await CreateHttpClient(bearerToken).PostAsJsonAsync(uri, request, cancellationToken);
                var httpResponse = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                if (httpResponse.Contains(typeof(Result).ToString()))
                    return JsonConvert.DeserializeObject<Result>(httpResponse);
                if (httpResponseMessage.IsSuccessStatusCode)
                    return Success(Created);
                return Error(httpResponse, httpResponseMessage.StatusCode);
            }
            catch (Exception exception)
            {
                LogService.LogError($"HttpServiceException: {GetType().Name}.Create(Uri = {uri}): {exception.Message}");
                return Error(HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task<Result> Update<TRequest>(string uri, TRequest request, string bearerToken = default, CancellationToken cancellationToken = default)
            where TRequest : class
        {
            try
            {
                var httpResponseMessage = await CreateHttpClient(bearerToken).PutAsJsonAsync(uri, request, cancellationToken);
                var httpResponse = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                if (httpResponse.Contains(typeof(Result).ToString()))
                    return JsonConvert.DeserializeObject<Result>(httpResponse);
                if (httpResponseMessage.IsSuccessStatusCode)
                    return Success(Updated);
                return Error(httpResponse, httpResponseMessage.StatusCode);
            }
            catch (Exception exception)
            {
                LogService.LogError($"HttpServiceException: {GetType().Name}.Update(Uri = {uri}): {exception.Message}");
                return Error(HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task<Result> Delete(string uri, int id, string bearerToken = default, CancellationToken cancellationToken = default)
        {
            try
            {
                var httpResponseMessage = await CreateHttpClient(bearerToken).DeleteAsync($"{uri}/{id}", cancellationToken);
                var httpResponse = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                if (httpResponse.Contains(typeof(Result).ToString()))
                    return JsonConvert.DeserializeObject<Result>(httpResponse);
                if (httpResponseMessage.IsSuccessStatusCode)
                    return Success(Deleted);
                return Error(httpResponse, httpResponseMessage.StatusCode);
            }
            catch (Exception exception)
            {
                LogService.LogError($"HttpServiceException: {GetType().Name}.Delete(Uri = {uri}, Id = {id}): {exception.Message}");
                return Error(HttpStatusCode.InternalServerError);
            }
        }
    }
}
