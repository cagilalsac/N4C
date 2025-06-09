namespace N4C.Models
{
    public class ControllerConfig : Config
    {
        public Dictionary<string, object> ViewData { get; private set; } = new Dictionary<string, object>();

        public void AddViewData(string key, object item)
        {
            ViewData.Add(key, item);
        }
    }
}
