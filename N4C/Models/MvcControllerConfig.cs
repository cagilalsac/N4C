using N4C.Extensions;

namespace N4C.Models
{
    public class MvcControllerConfig
    {
        public Dictionary<string, object> ViewData { get; private set; } = new Dictionary<string, object>();

        public string UriDictionaryKey { get; private set; }

        public void SetApi(string uriDictionaryKey = default)
        {
            UriDictionaryKey = uriDictionaryKey;
        }

        public Uri GetApi(string uriDictionaryKey = default)
        {
            return UriDictionaryKey.HasAny() ? uriDictionaryKey.GetUri() : null;
        }

        public void AddViewData(string key, object item)
        {
            ViewData.Add(key, item);
        }
    }
}
