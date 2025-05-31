using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using N4C.Services;

namespace N4C
{
    public static class IOC
    {
        public static void ConfigureN4C(this WebApplicationBuilder builder)
        {
            // Inversion of Control for HttpContext:
            builder.Services.AddHttpContextAccessor();

            // Inversion of Control for Services:
            builder.Services.AddScoped<Service>();

            // Session:
            if (Settings.SessionExpirationInMinutes > 0)
                builder.Services.AddSession(config => config.IdleTimeout = TimeSpan.FromMinutes(Settings.SessionExpirationInMinutes));
        }

        public static void ConfigureN4C(this WebApplication application)
        {
            // AppSettings:
            Settings.Development = application.Environment.IsDevelopment();

            // Session:
            if (Settings.SessionExpirationInMinutes > 0)
                application.UseSession();
        }
    }
}
