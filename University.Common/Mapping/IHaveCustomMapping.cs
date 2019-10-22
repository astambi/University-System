namespace University.Common.Mapping
{
    using AutoMapper;

    public interface IHaveCustomMapping
    {
        void ConfigureMapping(IProfileExpression mapper);
    }
}
