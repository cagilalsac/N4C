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

        public void SetUri(string uriPath, string uriOrigin, string token = default)
        {
            ApiUri = uriOrigin;
            Token = token;
            if (ApiUri.HasAny())
                Uri = new Uri($"{ApiUri}/{uriPath}");
        }

        public void SetTokenUri(string tokenUriOrigin)
        {
            if (tokenUriOrigin.HasAny())
            {
                TokenUri = new Uri($"{tokenUriOrigin}/Token");
                RefreshTokenUri = new Uri($"{tokenUriOrigin}/RefreshToken");
            }
        }

        public void AddViewData(string key, object item)
        {
            ViewData.Add(key, item);
        }
    }
}
