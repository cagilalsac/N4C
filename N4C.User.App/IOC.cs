using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using N4C.Models;
using N4C.Services;
using N4C.User.App.Domain;
using N4C.User.App.Models;
using N4C.User.App.Services;

namespace N4C.User.App
{
    public static class IOC
    {
        public static void ConfigureN4CUser(this WebApplicationBuilder builder)
        {
            // Inversion of Control for DbContext:
            builder.Services.AddDbContext<N4CUserDb>(options => options.UseSqlServer(builder.Configuration.GetConnectionString(nameof(N4CUserDb))));

            // Inversion of Control for Services:
            builder.Services.AddScoped<Service<N4CRole, N4CRoleRequest, N4CRoleResponse>, N4CRoleService>();
            builder.Services.AddScoped<Service<N4CStatus, N4CStatusRequest, N4CStatusResponse>, N4CStatusService>();
            builder.Services.AddScoped<Service<N4CUser, N4CUserRequest, N4CUserResponse>, N4CUserService>();

            // AppSettings:
            var appSettings = new N4CAppSettings(builder.Configuration, Defaults.TR, 30, 60,
                1, 24 * 60, true, "https://need4code.com", "https://need4code.com", "4QrJRmIu0R9PlAGrGgQAi6OJ5cf5VZNf", SecurityAlgorithms.HmacSha256Signature);
            appSettings.Bind();

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
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // N4C:
            builder.ConfigureN4C();
        }

        public static void ConfigureN4CUser(this WebApplication application)
        {
            // Authentication:
            application.UseAuthentication();

            // N4C:
            application.ConfigureN4C();
        }
    }
}
