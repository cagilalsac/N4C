using APP.Domain;
using APP.Services;
using APP.Services.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using N4C.App;
using N4C.App.Services;
using N4C.Domain;

namespace APP
{
    public static class IOC
    {
        public static void ConfigureApp(this WebApplicationBuilder builder, string culture)
        {
            // Inversion of Control for DbContext:
            builder.Services.AddDbContext<IDb, Db>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Db")));

            // Inversion of Control for Services:
            builder.Services.AddScoped<Service<Category, CategoryRequest, CategoryResponse>, CategoryService>();
            builder.Services.AddScoped<Service<Store, StoreRequest, StoreResponse>, StoreService>();
            builder.Services.AddScoped<Service<Product, ProductRequest, ProductResponse>, ProductService>();

            // AppSettings:
            var appSettings = new AppSettings(builder.Configuration, culture, 20, 30);
            appSettings.Bind();

            // N4C:
            builder.ConfigureN4C();
        }

        public static void ConfigureApp(this WebApplication application)
        {
            // N4C:
            application.ConfigureN4C();
        }
    }
}
