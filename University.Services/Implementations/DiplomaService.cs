namespace University.Services.Implementations
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.EntityFrameworkCore;
    using University.Data;
    using University.Services.Models.Diplomas;

    public class DiplomaService : IDiplomaService
    {
        private readonly UniversityDbContext db;
        private readonly IMapper mapper;

        public DiplomaService(
            UniversityDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<DiplomaServiceModel> GetByIdAsync(string id)
            => await this.db
            .Diplomas
            .Where(d => d.Id == id)
            .ProjectTo<DiplomaServiceModel>(this.mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }
}
