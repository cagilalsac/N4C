using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using N4C.Models;
using System.Text;

namespace N4C
{
    public abstract class Settings
    {
        protected virtual string AppSettingsSection => "AppSettings";

        public static string Culture { get; private set; }

        public static int SessionExpirationInMinutes { get; private set; }
        public static int AuthCookieExpirationInMinutes { get; private set; }

        public static int JwtExpirationInMinutes { get; private set; }
        public static string JwtAudience { get; private set; }
        public static string JwtIssuer { get; private set; }
        public static string JwtSecurityKey { get; private set; }
        public static string JwtSecurityAlgorithm { get; private set; }

        public static SecurityKey JwtSigningKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecurityKey));

        public static bool Development { get; internal set; }

        private readonly IConfiguration _configuration;

        protected Settings(IConfiguration configuration, string culture = default, int sessionExpirationInMinutes = 30, int authCookieExpirationInMinutes = 60,
            int jwtExpirationInMinutes = 5, string jwtAudience = default, string jwtIssuer = default, string jwtSecurityKey = default, string jwtSecurityAlgorithm = default)
        {
            _configuration = configuration;
            Culture = culture ?? Defaults.TR;
            SessionExpirationInMinutes = sessionExpirationInMinutes;
            AuthCookieExpirationInMinutes = authCookieExpirationInMinutes;
            JwtExpirationInMinutes = jwtExpirationInMinutes;
            JwtAudience = jwtAudience ?? "https://need4code.com";
            JwtIssuer = jwtIssuer ?? "https://need4code.com";
            JwtSecurityKey = jwtSecurityKey ?? "4QrJRmIu0R9PlAGrGgQAi6OJ5cf5VZNf";
            JwtSecurityAlgorithm = jwtSecurityAlgorithm ?? SecurityAlgorithms.HmacSha256Signature;
        }

        public void Bind() => _configuration.GetSection(AppSettingsSection).Bind(this);
    }
}
