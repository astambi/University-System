namespace LearningSystem.Web.Infrastructure.Mapping
{
    using System;
    using System.Linq;
    using System.Reflection;
    using AutoMapper;
    using LearningSystem.Common.Mapping;

    public class AutoMapperProfile : Profile
    {
        private const string ModelSuffix = "model";
        private readonly string SolutionAssemblyName;

        public AutoMapperProfile()
        {
            this.SolutionAssemblyName = Assembly
                .GetExecutingAssembly()
                .GetName()
                .Name // LearningSystem.Web
                .Split('.')
                .First(); // LearningSystem

            var assemblies = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(a => a.GetName().Name.Contains(this.SolutionAssemblyName))
                .ToList();

            var modelTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass
                    && !t.IsAbstract
                    && t.IsPublic
                    && t.Name.ToLower().EndsWith(ModelSuffix)) // models
                .ToList();

            var destinationTypes = modelTypes
                .Where(t => t
                    .GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Select(i => i.GetGenericTypeDefinition())
                    .Contains(typeof(IMapFrom<>))) // IMapFrom<>
                .ToList();

            // Register IMapFrom mappings
            destinationTypes
                .Select(t => new
                {
                    Destination = t,
                    Source = t
                        .GetInterfaces()
                        .Where(i => i.IsGenericType)
                        .Select(i => new
                        {
                            Definition = i.GetGenericTypeDefinition(),
                            Arguments = i.GetGenericArguments()
                        })
                        .Where(i => i.Definition == typeof(IMapFrom<>))
                        .SelectMany(i => i.Arguments)
                        .First()
                })
                .ToList()
                .ForEach(mapping => this.CreateMap(mapping.Source, mapping.Destination));

            // Register IHaveCustomMapping mappings
            modelTypes
                .Where(t => typeof(IHaveCustomMapping).IsAssignableFrom(t))
                .Select(Activator.CreateInstance)
                .Cast<IHaveCustomMapping>()
                .ToList()
                .ForEach(mapping => mapping.ConfigureMapping(this));
        }
    }
}
