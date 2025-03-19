using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace N4C.App
{
    public abstract class Settings
    {
        protected virtual string AppSettingsSection => "AppSettings";

        public static string Culture { get; private set; } = Cultures.EN;

        public static bool Development { get; internal set; }

        public static int SessionExpirationInMinutes { get; private set; }
        public static double AuthCookieExpirationInHours { get; private set; }

        public static double JwtExpirationInHours { get; private set; }
        public static string JwtAudience { get; private set; }
        public static string JwtIssuer { get; private set; }
        public static string JwtSecurityKey { get; private set; }
        public static string JwtSecurityAlgorithm { get; private set; }

        public static SecurityKey JwtSigningKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecurityKey));

        private readonly IConfiguration _configuration;

        protected Settings(IConfiguration configuration, string culture, double jwtExpirationInHours, 
            string jwtAudience, string jwtIssuer, string jwtSecurityKey, string jwtSecurityAlgorithm)
        {
            _configuration = configuration;
            Culture = culture;
            JwtExpirationInHours = jwtExpirationInHours;
            JwtAudience = jwtAudience;
            JwtIssuer = jwtIssuer;
            JwtSecurityKey = string.IsNullOrWhiteSpace(jwtSecurityKey) ? "4QrJRmIu0R9PlAGrGgQAi6OJ5cf5VZNf" : jwtSecurityKey;
            JwtSecurityAlgorithm = string.IsNullOrWhiteSpace(jwtSecurityAlgorithm) ? SecurityAlgorithms.HmacSha256Signature : jwtSecurityAlgorithm;
        }

        protected Settings(IConfiguration configuration, string culture, int sessionExpirationInMinutes, double authCookieExpirationInHours,
            double jwtExpirationInHours, string jwtAudience = "https://n4c.com", string jwtIssuer = "https://n4c.com", 
            string jwtSecurityKey = default, string jwtSecurityAlgorithm = default)
            : this(configuration, culture, jwtExpirationInHours, jwtAudience, jwtIssuer, jwtSecurityKey, jwtSecurityAlgorithm)
        {
            SessionExpirationInMinutes = sessionExpirationInMinutes;
            AuthCookieExpirationInHours = authCookieExpirationInHours;
        }

        public void Bind() => _configuration.GetSection(AppSettingsSection).Bind(this);
    }
}
