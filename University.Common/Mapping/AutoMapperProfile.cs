namespace University.Common.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoMapper;

    public class AutoMapperProfile : Profile
    {
        private const string ModelSuffix = "model";

        public AutoMapperProfile()
        {
            var modelTypes = this.GetModelTypes();

            this.RegisterFromMappings(modelTypes);
            this.RegisterCustomMappings(modelTypes);
        }

        private void RegisterCustomMappings(IEnumerable<Type> modelTypes)
        {
            var mappings = modelTypes
                .Where(t => typeof(IHaveCustomMapping).IsAssignableFrom(t))
                .Select(Activator.CreateInstance)
                .Cast<IHaveCustomMapping>()
                .ToList();

            mappings.ForEach(mapping => mapping.ConfigureMapping(this));
        }

        private void RegisterFromMappings(IEnumerable<Type> modelTypes)
        {
            var destinationTypes = modelTypes
                .Where(t => t
                    .GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Select(i => i.GetGenericTypeDefinition())
                    .Contains(typeof(IMapFrom<>)))
                .ToList();

            var mappings = destinationTypes
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
                .ToList();

            mappings.ForEach(mapping => this.CreateMap(mapping.Source, mapping.Destination));
        }

        private IEnumerable<Type> GetModelTypes()
        {
            var solutionAssemblyName = Assembly
                .GetExecutingAssembly()
                .GetName()
                .Name // University.Web
                .Split('.')
                .First(); // University

            var assemblies = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(a => a.GetName().Name.Contains(solutionAssemblyName))
                .ToList();

            var modelTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass
                    && !t.IsAbstract
                    && t.IsPublic
                    && t.Name.ToLower().EndsWith(ModelSuffix)) // models
                .ToList();

            return modelTypes;
        }
    }
}