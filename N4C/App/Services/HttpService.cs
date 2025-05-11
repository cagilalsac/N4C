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
        protected HttpServiceConfig Config { get; private set; } = new HttpServiceConfig();

        protected string Token => Config.Token;

        public string ApiUri => Config.ApiUri;

        private IHttpContextAccessor HttpContextAccessor { get; }
        private IHttpClientFactory HttpClientFactory { get; }

        public HttpService(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, LogService logService) : base(logService)
        {
            HttpContextAccessor = httpContextAccessor;
            HttpClientFactory = httpClientFactory;
            Set(GetCookie("Culture") ?? Config.Culture, Config.TitleTR, Config.TitleEN);
        }

        public void Set(Action<HttpServiceConfig> config)
        {
            config.Invoke(Config);
            Set(Config.Culture, Config.TitleTR, Config.TitleEN);
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

        public string GetToken(List<Claim> claims, DateTime? expiration = default)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.JwtSecurityKey));
            var signingCredentials = new SigningCredentials(securityKey, Settings.JwtSecurityAlgorithm);
            var jwtSecurityToken = new JwtSecurityToken(Settings.JwtIssuer, Settings.JwtAudience, claims, DateTime.Now, expiration, signingCredentials);
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            return jwtSecurityTokenHandler.WriteToken(jwtSecurityToken);
        }

        protected string GetToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                token = HttpContextAccessor.HttpContext?.Request?.Headers?.Authorization.FirstOrDefault() ?? GetCookie("Token") ?? string.Empty;
            if (token.StartsWith(JwtBearerDefaults.AuthenticationScheme))
                token = token.Remove(0, JwtBearerDefaults.AuthenticationScheme.Length).TrimStart();
            return token;
        }

        protected HttpClient GetHttpClient(string token = default)
        {
            var httpClient = HttpClientFactory.CreateClient();
            token = GetToken(token);
            if (!string.IsNullOrWhiteSpace(token))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
            return httpClient;
        }

        public virtual async Task<Result<List<TResponse>>> Get<TResponse>(CancellationToken cancellationToken = default)
            where TResponse : class, new()
        {
            if (string.IsNullOrWhiteSpace(ApiUri))
                return null;
            List<TResponse> list = null;
            try
            {
                Result<List<TResponse>> result = null;
                var httpResponseMessage = await GetHttpClient(Token).GetAsync(ApiUri, cancellationToken);
                var httpResponse = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                if (httpResponse.Contains(typeof(Result).ToString()))
                    result = JsonConvert.DeserializeObject<Result<List<TResponse>>>(httpResponse);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    list = result?.Data ?? JsonConvert.DeserializeObject<List<TResponse>>(httpResponse);
                    if (list is not null && list.Any())
                        return Success(list, $"{list.Count} {Found}");
                    return Error(list, HttpStatusCode.NotFound);
                }
                return Error(list, httpResponse, httpResponseMessage.StatusCode);
            }
            catch (Exception exception)
            {
                LogError($"HttpServiceException: {GetType().Name}.Get(Uri = {ApiUri}): {exception.Message}");
                return Error(list, HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task<Result<TResponse>> Get<TResponse>(int id, CancellationToken cancellationToken = default)
            where TResponse : class, new()
        {
            if (string.IsNullOrWhiteSpace(ApiUri))
                return null;
            TResponse item = null;
            try
            {
                Result<TResponse> result = null;
                var httpResponseMessage = await GetHttpClient(Token).GetAsync($"{ApiUri}/{id}", cancellationToken);
                var httpResponse = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                if (httpResponse.Contains(typeof(Result).ToString()))
                    result = JsonConvert.DeserializeObject<Result<TResponse>>(httpResponse);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    item = result?.Data ?? JsonConvert.DeserializeObject<TResponse>(httpResponse);
                    if (item is not null)
                        return Success(item, $"1 {Found}", HttpStatusCode.PartialContent);
                    return Error(item, HttpStatusCode.NotFound);
                }
                return Error(item, httpResponse, httpResponseMessage.StatusCode);
            }
            catch (Exception exception)
            {
                LogError($"HttpServiceException: {GetType().Name}.Get(Uri = {ApiUri}/{id}): {exception.Message}");
                return Error(item, HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task<Result> Create<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : class, new()
        {
            if (string.IsNullOrWhiteSpace(ApiUri))
                return null;
            try
            {
                Result result = null;
                var httpResponseMessage = await GetHttpClient(Token).PostAsJsonAsync(ApiUri, request, cancellationToken);
                var httpResponse = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                if (httpResponse.Contains(typeof(Result).ToString()))
                    result = JsonConvert.DeserializeObject<Result>(httpResponse);
                if (httpResponseMessage.IsSuccessStatusCode)
                    return Success(result is null ? Created : result.Message, result?.Id);
                return Error(result is null ? httpResponse : result.Message, httpResponseMessage.StatusCode);
            }
            catch (Exception exception)
            {
                LogError($"HttpServiceException: {GetType().Name}.Create(request, Uri = {ApiUri}): {exception.Message}");
                return Error(HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task<Result> Update<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : class, new()
        {
            if (string.IsNullOrWhiteSpace(ApiUri))
                return null;
            try
            {
                Result result = null;
                var httpResponseMessage = await GetHttpClient(Token).PutAsJsonAsync(ApiUri, request, cancellationToken);
                var httpResponse = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                if (httpResponse.Contains(typeof(Result).ToString()))
                    result = JsonConvert.DeserializeObject<Result>(httpResponse);
                if (httpResponseMessage.IsSuccessStatusCode)
                    return Success(result is null ? Updated : result.Message, result?.Id);
                return Error(result is null ? httpResponse : result.Message, httpResponseMessage.StatusCode);
            }
            catch (Exception exception)
            {
                LogError($"HttpServiceException: {GetType().Name}.Update(request, Uri = {ApiUri}): {exception.Message}");
                return Error(HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task<Result> Delete(int id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ApiUri))
                return null;
            try
            {
                Result result = null;
                var httpResponseMessage = await GetHttpClient(Token).DeleteAsync($"{ApiUri}/{id}", cancellationToken);
                var httpResponse = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                if (httpResponse.Contains(typeof(Result).ToString()))
                    result = JsonConvert.DeserializeObject<Result>(httpResponse);
                if (httpResponseMessage.IsSuccessStatusCode)
                    return Success(result is null ? Deleted : result.Message, id);
                return Error(result is null ? httpResponse : result.Message, httpResponseMessage.StatusCode);
            }
            catch (Exception exception)
            {
                LogError($"HttpServiceException: {GetType().Name}.Delete(Uri = {ApiUri}/{id}): {exception.Message}");
                return Error(HttpStatusCode.InternalServerError);
            }
        }
    }
}
