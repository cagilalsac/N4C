using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using N4C.Extensions;
using N4C.Models;
using System.Text;

namespace N4C
{
    public abstract class Settings
    {
        protected virtual string AppSettingsSection => "AppSettings";

        public static string Culture { get; set; }

        public static int SessionExpirationInMinutes { get; set; }
        public static int AuthCookieExpirationInMinutes { get; set; }

        public static int JwtExpirationInMinutes { get; set; }
        public static string JwtAudience { get; set; }
        public static string JwtIssuer { get; set; }
        public static string JwtSecurityKey { get; private set; }
        public static string JwtSecurityAlgorithm { get; private set; }

        public static SecurityKey JwtSigningKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecurityKey));

        public static int RefreshTokenExpirationInMinutes { get; set; }
        public static bool RefreshTokenSlidingExpiration { get; set; }

        public static Dictionary<string, string> UriDictionary { get; set; }

        public static bool Development { get; internal set; }

        private readonly IConfiguration _configuration;

        protected Settings(IConfiguration configuration, string jwtSecurityKey = default, string jwtSecurityAlgorithm = default)
        {
            _configuration = configuration;
            Culture = Defaults.TR;
            SessionExpirationInMinutes = 30;
            AuthCookieExpirationInMinutes = 60;
            JwtExpirationInMinutes = 5;
            JwtAudience = "https://need4code.com";
            JwtIssuer = "https://need4code.com";
            JwtSecurityKey = jwtSecurityKey.HasNotAny("4QrJRmIu0R9PlAGrGgQAi6OJ5cf5VZNf");
            JwtSecurityAlgorithm = jwtSecurityAlgorithm.HasNotAny(SecurityAlgorithms.HmacSha256Signature);
            RefreshTokenExpirationInMinutes = 1440;
            RefreshTokenSlidingExpiration = true;
        }

        public void Bind() => _configuration.GetSection(AppSettingsSection).Bind(this);
    }
}
