namespace LearningSystem.Services.Admin.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using LearningSystem.Data;
    using LearningSystem.Services.Admin.Models;
    using Microsoft.EntityFrameworkCore;

    public class AdminUserService : IAdminUserService
    {
        private readonly LearningSystemDbContext db;
        private readonly IMapper mapper;

        public AdminUserService(LearningSystemDbContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<AdminUserListingServiceModel>> AllAsync()
            => await this.mapper
            .ProjectTo<AdminUserListingServiceModel>(this.db.Users)
            .OrderBy(u => u.Username)
            .ToListAsync();
    }
}
