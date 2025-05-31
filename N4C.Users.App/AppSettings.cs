using Microsoft.Extensions.Configuration;

namespace N4C.Users.App
{
    public class AppSettings : Settings
    {
        public static string Title { get; set; }
        public static string DescriptionEN { get; set; }
        public static string DescriptionTR { get; set; }

        public AppSettings(IConfiguration configuration, string culture = null, int sessionExpirationInMinutes = 20) : base(configuration, culture, sessionExpirationInMinutes)
        {
        }
    }
}
