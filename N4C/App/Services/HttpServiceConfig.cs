namespace N4C.App.Services
{
    public class HttpServiceConfig : IServiceConfig
    {
        public string Culture { get; set; } = Settings.Culture;
        public string TitleTR { get; set; } = "Kayıt";
        public string TitleEN { get; set; } = "Record";
        public string Token { get; set; }

        private string _apiUri;
        public string ApiUri 
        {
            get => _apiUri;
            set => _apiUri = (value ?? string.Empty).ToLower().StartsWith("http") ? value : $"{Settings.ApiUri}/{value}"; 
        }
    }
}
