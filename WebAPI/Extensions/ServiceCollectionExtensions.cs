using Microsoft.AspNetCore.Mvc;
using WebAPI.Middlewares;

namespace WebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            // Configure API Behavior Options
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true; // We'll handle validation manually
            });

            // Add custom filters
            services.AddScoped<ValidateModelAttribute>();

            return services;
        }
    }
}