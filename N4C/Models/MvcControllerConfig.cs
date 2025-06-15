namespace N4C.Models
{
    public class MvcControllerConfig
    {
        public string Culture { get; private set; } = Settings.Culture;
        public Dictionary<string, object> ViewData { get; private set; } = new Dictionary<string, object>();

        public void SetCulture(string culture)
        {
            Culture = culture;
        }

        public void AddViewData(string key, object item)
        {
            ViewData.Add(key, item);
        }
    }
}
