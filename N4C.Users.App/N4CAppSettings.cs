using Microsoft.Extensions.Configuration;

namespace N4C.Users.App
{
    public class N4CAppSettings : Settings
    {
        protected override string AppSettingsSection => nameof(N4CAppSettings);

        public static string Title { get; set; }
        public static string DescriptionEN { get; set; }
        public static string DescriptionTR { get; set; }
        public static int AuthCookieExpirationInMinutes { get; private set; }

        public N4CAppSettings(IConfiguration configuration, string culture = null, int sessionExpirationInMinutes = 20, int authCookieExpirationInMinutes = 30)
            : base(configuration, culture, sessionExpirationInMinutes)
        {
            AuthCookieExpirationInMinutes = authCookieExpirationInMinutes;
        }
    }
}
