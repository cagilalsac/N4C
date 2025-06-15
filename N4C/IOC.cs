using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using N4C.Services;

namespace N4C
{
    public static class IOC
    {
        public static void ConfigureN4C(this WebApplicationBuilder builder)
        {
            // Inversion of Control for HttpContext:
            builder.Services.AddHttpContextAccessor();

            // Inversion of Control for HttpClient:
            builder.Services.AddHttpClient();

            // Inversion of Control for Services:
            builder.Services.AddScoped<Service>();

            // Session:
            if (Settings.SessionExpirationInMinutes > 0)
                builder.Services.AddSession(config => config.IdleTimeout = TimeSpan.FromMinutes(Settings.SessionExpirationInMinutes));

            // Authentication:
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, config =>
                {
                    config.LoginPath = "/Login";
                    config.AccessDeniedPath = "/Login";
                    config.SlidingExpiration = true;
                    config.ExpireTimeSpan = TimeSpan.FromMinutes(Settings.AuthCookieExpirationInMinutes);
                })
                .AddJwtBearer(config =>
                {
                    config.TokenValidationParameters = new TokenValidationParameters()
                    {
                        IssuerSigningKey = Settings.JwtSigningKey,
                        ValidIssuer = Settings.JwtIssuer,
                        ValidAudience = Settings.JwtAudience,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true
                    };
                });

            // API ModelState:
            builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

            // API CORS:
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });
        }

        public static void ConfigureN4C(this WebApplication application)
        {
            // AppSettings:
            Settings.Development = application.Environment.IsDevelopment();

            // Session:
            if (Settings.SessionExpirationInMinutes > 0)
                application.UseSession();

            // Authentication:
            application.UseAuthentication();

            // API CORS:
            application.UseCors();
        }
    }
}
