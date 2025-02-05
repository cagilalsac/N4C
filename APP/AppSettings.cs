using Microsoft.Extensions.Configuration;
using N4C.App;

namespace APP
{
    public class AppSettings : Settings
    {
        public static string Title { get; set; }
        public static string DescriptionEN { get; set; }
        public static string DescriptionTR { get; set; }

        public AppSettings(IConfiguration configuration, int sessionExpirationInMinutes, string culture) 
            : base(configuration, sessionExpirationInMinutes, culture)
        {
        }
    }
}
