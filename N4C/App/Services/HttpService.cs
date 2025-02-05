using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using N4C.Domain;
using System.Security.Claims;
using System.Text.Json;

namespace N4C.App.Services
{
    public class HttpService
    {
        public string UserName => HttpContextAccessor.HttpContext?.User.Identity?.Name;
        public int UserId => Convert.ToInt32(HttpContextAccessor.HttpContext?.User.Claims?.SingleOrDefault(claim => claim.Type == nameof(Entity.Id)).Value);

        protected IHttpContextAccessor HttpContextAccessor { get; }

        public HttpService(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        public T GetSession<T>(string key) where T : class
        {
            var serializedBytes = HttpContextAccessor.HttpContext.Session.Get(key);
            if (serializedBytes is null)
                return null;
            return JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(serializedBytes));
        }

        public void SetSession<T>(string key, T instance) where T : class
        {
            var serializedBytes = JsonSerializer.SerializeToUtf8Bytes(instance);
            HttpContextAccessor.HttpContext.Session.Set(key, serializedBytes);
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

        public async Task SignInAsync(List<Claim> claims, string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme, bool isPersistent = true)
        {
            var identity = new ClaimsIdentity(claims, authenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authenticationProperties = new AuthenticationProperties() { IsPersistent = isPersistent };
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
    }
}
