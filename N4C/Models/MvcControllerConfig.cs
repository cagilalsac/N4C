using N4C.Extensions;

namespace N4C.Models
{
    public class MvcControllerConfig
    {
        public Dictionary<string, object> ViewData { get; private set; } = new Dictionary<string, object>();

        public string UriDictionaryKey { get; private set; }

        public void SetUri(bool api, string uriDictionaryKey = "")
        {
            UriDictionaryKey = api ? uriDictionaryKey : null;
        }

        public Uri GetUri(string otherUriDictionaryKey)
        {
            return UriDictionaryKey.HasAny() ? otherUriDictionaryKey.GetUri() : null;
        }

        public void AddViewData(string key, object item)
        {
            ViewData.Add(key, item);
        }
    }
}
