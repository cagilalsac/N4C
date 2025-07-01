using N4C.Extensions;

namespace N4C.Models
{
    public class MvcControllerConfig
    {
        public Dictionary<string, object> ViewData { get; private set; } = new Dictionary<string, object>();

        public string ApiUri { get; private set; }
        public Uri Uri { get; private set; }
        public Uri TokenUri { get; private set; }
        public Uri RefreshTokenUri { get; private set; }
        public string Token { get; private set; }

        public void SetUri(string path, string origin, string token = default)
        {
            ApiUri = origin;
            if (ApiUri.HasAny())
            {
                Uri = new Uri($"{ApiUri}/{path}");
                Token = token;
                if (Token.HasNotAny())
                    SetRefreshTokenUri();
            }
        }

        public void SetRefreshTokenUri(string path = default, string origin = default)
        {
            path = path.HasNotAny("RefreshToken");
            origin = origin.HasNotAny(ApiUri);
            if (origin.HasAny())
                RefreshTokenUri = new Uri($"{origin}/{path}");
        }

        public void SetTokenUri(string path = default, string origin = default)
        {
            path = path.HasNotAny("Token");
            origin = origin.HasNotAny(ApiUri);
            if (origin.HasAny())
                TokenUri = new Uri($"{origin}/{path}");
        }

        public void AddViewData(string key, object item)
        {
            ViewData.Add(key, item);
        }
    }
}
