using Microsoft.Extensions.Configuration;

namespace N4C.Users.App
{
    public class N4CAppSettings : Settings
    {
        protected override string AppSettingsSection => nameof(N4CAppSettings);

        public static string Title { get; set; }
        public static string DescriptionEN { get; set; }
        public static string DescriptionTR { get; set; }

        public N4CAppSettings(IConfiguration configuration, string culture = null, int sessionExpirationInMinutes = 30, int authCookieExpirationInMinutes = 60, 
            int jwtExpirationInMinutes = 5, int refreshTokenExpirationInMinutes = 1440, string jwtAudience = null, string jwtIssuer = null, 
            string jwtSecurityKey = null, string jwtSecurityAlgorithm = null) 
            : base(configuration, culture, sessionExpirationInMinutes, authCookieExpirationInMinutes, 
                  jwtExpirationInMinutes, refreshTokenExpirationInMinutes, jwtAudience, jwtIssuer, jwtSecurityKey, jwtSecurityAlgorithm)
        {
        }
    }
}
