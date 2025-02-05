using Microsoft.Extensions.Configuration;

namespace N4C.App
{
    public abstract class Settings
    {
        protected virtual string AppSettingsSection => "AppSettings";

        public static int SessionExpirationInMinutes { get; private set; }
        public static string Culture { get; private set; }
        public static bool Development { get; internal set; }

        private readonly IConfiguration _configuration;

        protected Settings(IConfiguration configuration, int sessionExpirationInMinutes, string culture)
        {
            _configuration = configuration;
            SessionExpirationInMinutes = sessionExpirationInMinutes;
            Culture = culture;
        }

        public void Bind() => _configuration.GetSection(AppSettingsSection).Bind(this);
    }
}
