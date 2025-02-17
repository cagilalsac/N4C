using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using N4C.App.Services;
using N4C.App.Services.Users;
using N4C.App.Services.Users.Models;
using N4C.Domain.Users;

namespace N4C.App
{
    public static class IOC
    {
        public static void ConfigureN4C(this WebApplicationBuilder builder)
        {
            // Inversion of Control for HttpContext:
            builder.Services.AddHttpContextAccessor();

            // Inversion of Control for Services:
            builder.Services.AddScoped<HttpService>();
            builder.Services.AddScoped<Application>();
            builder.Services.AddScoped<Service<Role, RoleRequest, RoleResponse>, RoleService>();

            // MediatR:
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblies(assembly));
            }

            // Session:
            if (Settings.SessionExpirationInMinutes > 0)
                builder.Services.AddSession(config => config.IdleTimeout = TimeSpan.FromMinutes(Settings.SessionExpirationInMinutes));

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

            // API CORS:
            application.UseCors();
        }
    }
}
