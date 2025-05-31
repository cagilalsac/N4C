using Microsoft.Extensions.Configuration;
using N4C.Models;

namespace N4C
{
    public abstract class Settings
    {
        protected virtual string AppSettingsSection => "AppSettings";

        public static string Culture { get; private set; }
        public static int SessionExpirationInMinutes { get; private set; }

        public static bool Development { get; internal set; }

        private readonly IConfiguration _configuration;

        protected Settings(IConfiguration configuration, string culture = default, int sessionExpirationInMinutes = 20)
        {
            _configuration = configuration;
            Culture = culture ?? Cultures.TR;
            SessionExpirationInMinutes = sessionExpirationInMinutes;
        }

        public void Bind() => _configuration.GetSection(AppSettingsSection).Bind(this);
    }
}
