namespace University.Web.Infrastructure.Extensions
{
    using System.Linq;
    using System.Reflection;
    using CloudinaryDotNet;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using University.Services;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureIdentityOptions(this IServiceCollection services)
        {
            services
                .Configure<IdentityOptions>(options =>
                {
                    // Password settings
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    // User settings
                    options.User.RequireUniqueEmail = true;
                });

            return services;
        }

        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            var domainServices = Assembly
                 .GetAssembly(typeof(IService))
                 .GetTypes()
                 .Where(t => t.IsClass
                          && t.GetInterfaces().Any(i => i.Name == $"I{t.Name}"))
                 .Select(t => new
                 {
                     Interface = t.GetInterface($"I{t.Name}"),
                     Implementation = t,
                     IsSingleton = t.GetInterfaces().Any(i => i == typeof(ISingletonService))
                 })
                 .ToList();

            foreach (var service in domainServices)
            {
                if (service.IsSingleton)
                {
                    services.AddSingleton(service.Interface, service.Implementation);
                }
                else
                {
                    services.AddTransient(service.Interface, service.Implementation);
                }
            }

            return services;
        }

        public static IServiceCollection AddCloudinaryService(this IServiceCollection services, string cloudName, string apiKey, string apiSecret)
        {
            if (string.IsNullOrWhiteSpace(cloudName)
                || string.IsNullOrWhiteSpace(apiKey)
                || string.IsNullOrWhiteSpace(apiSecret))
            {
                return services;
            }

            var cloudinaryCredentials = new Account(cloudName, apiKey, apiSecret);
            var cloudinary = new Cloudinary(cloudinaryCredentials);

            services.AddSingleton(cloudinary); // Singleton

            return services;
        }

        public static IServiceCollection AddRoutingOptions(this IServiceCollection services)
        {
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                /// NB! Lowercase query strings modify tokens sent via email 
                /// and prevent password confirmation / reset
                options.LowercaseQueryStrings = false;
            });

            return services;
        }

        public static IServiceCollection AddMvcOptions(this IServiceCollection services)
        {
            //services
            //    .AddMvc(options =>
            //    {
            //        // Global filters
            //        options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
            //    })
            //    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            //    // Identity
            //    .AddRazorPagesOptions(options =>
            //    {
            //        options.AllowAreas = true;
            //        options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
            //        options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
            //    });

            services
                .AddControllersWithViews(options => options
                    .Filters.Add<AutoValidateAntiforgeryTokenAttribute>());

            services.AddRazorPages();

            return services;
        }

        public static IServiceCollection ConfigureApplicationCookieOptions(this IServiceCollection services)
        {
            // Identity
            services
                .ConfigureApplicationCookie(options =>
                {
                    options.LoginPath = $"/Identity/Account/Login";
                    options.LogoutPath = $"/Identity/Account/Logout";
                    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
                });

            return services;
        }
    }
}