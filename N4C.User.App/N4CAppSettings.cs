using Microsoft.Extensions.Configuration;

namespace N4C.User.App
{
    public class N4CAppSettings : Settings
    {
        protected override string AppSettingsSection => nameof(N4CAppSettings);

        public static string Title { get; set; }
        public static string DescriptionEN { get; set; }
        public static string DescriptionTR { get; set; }

        public N4CAppSettings(IConfiguration configuration, string jwtSecurityKey = null, string jwtSecurityAlgorithm = null) 
            : base(configuration, jwtSecurityKey, jwtSecurityAlgorithm)
        {
        }
    }
}
