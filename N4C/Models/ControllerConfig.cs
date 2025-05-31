namespace N4C.Models
{
    public class ControllerConfig : Config
    {
        public bool ModelStateErrors { get; private set; } = true;

        public Dictionary<string, object> ViewData { get; private set; } = new Dictionary<string, object>();

        public void SetModelStateErrors(bool modelStateErrors)
        {
            ModelStateErrors = modelStateErrors;
        }

        public void AddViewData(string key, object item)
        {
            ViewData.Add(key, item);
        }
    }
}
