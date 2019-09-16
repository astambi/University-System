namespace University.Web.Infrastructure.Extensions
{
    using System.Linq;
    using System.Reflection;
    using CloudinaryDotNet;
    using Microsoft.Extensions.DependencyInjection;
    using University.Services;

    public static class ServiceCollectionExtensions
    {
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
                     IsSingleton = t.GetInterfaces().Any(i => i.Name == nameof(ISingletonService))
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
    }
}