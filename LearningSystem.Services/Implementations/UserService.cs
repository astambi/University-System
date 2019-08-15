namespace LearningSystem.Services.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.Courses;
    using LearningSystem.Services.Models.Users;
    using Microsoft.EntityFrameworkCore;

    public class UserService : IUserService
    {
        private readonly LearningSystemDbContext db;
        private readonly IMapper mapper;

        public UserService(
            LearningSystemDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<bool> CanBeDeletedAsync(string id)
            => !await this.db.Courses.AnyAsync(c => c.TrainerId == id)
            && !await this.db.Articles.AnyAsync(a => a.AuthorId == id);

        public async Task<UserEditServiceModel> GetProfileToEditAsync(string id)
            => await this.mapper
            .ProjectTo<UserEditServiceModel>(this.GetUserById(id))
            .FirstOrDefaultAsync();

        public async Task<UserWithBirthdateServiceModel> GetUserProfileDataAsync(string id)
            => await this.mapper
            .ProjectTo<UserWithBirthdateServiceModel>(this.GetUserById(id))
            .FirstOrDefaultAsync();

        public async Task<IEnumerable<CourseProfileServiceModel>> GetUserProfileCoursesAsync(string id)
            => await this.mapper
            .ProjectTo<CourseProfileServiceModel>(
                this.GetUserById(id)
                .SelectMany(u => u.Courses))
            .OrderByDescending(c => c.CourseStartDate)
            .ThenByDescending(c => c.CourseEndDate)
            .ToListAsync();

        public async Task<bool> UpdateUserProfileAsync(string id, string name, DateTime birthdate)
        {
            var user = await this.db.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            user.Name = name.Trim();
            user.Birthdate = birthdate;

            var result = await this.db.SaveChangesAsync();
            var success = result > 0;

            return success;
        }

        private IQueryable<User> GetUserById(string id)
            => this.db.Users.Where(u => u.Id == id);
    }
}
