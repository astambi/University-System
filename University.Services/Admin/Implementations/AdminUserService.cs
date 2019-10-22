namespace University.Services.Admin.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.EntityFrameworkCore;
    using University.Data;
    using University.Services.Admin.Models.Users;

    public class AdminUserService : IAdminUserService
    {
        private readonly UniversityDbContext db;
        private readonly IMapper mapper;

        public AdminUserService(
            UniversityDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<AdminUserListingServiceModel>> AllAsync()
            => await this.db
            .Users
            .OrderBy(u => u.UserName)
            .ProjectTo<AdminUserListingServiceModel>(this.mapper.ConfigurationProvider)
            .ToListAsync();
    }
}
