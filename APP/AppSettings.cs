using Microsoft.Extensions.Configuration;
using N4C.App;

namespace APP
{
    public class AppSettings : Settings
    {
        public static string Title { get; set; }
        public static string DescriptionEN { get; set; }
        public static string DescriptionTR { get; set; }

        public AppSettings(IConfiguration configuration, string culture, int sessionExpirationInMinutes, double authCookieExpirationInHours, 
            double jwtExpirationInHours, string apiUri = default, string jwtAudience = "https://n4c.com", string jwtIssuer = "https://n4c.com", 
            string jwtSecurityKey = null, string jwtSecurityAlgorithm = null) 
            : base(configuration, culture, sessionExpirationInMinutes, authCookieExpirationInHours, jwtExpirationInHours, 
                  apiUri, jwtAudience, jwtIssuer, jwtSecurityKey, jwtSecurityAlgorithm)
        {
        }
    }
}
