namespace University.Services.Admin.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.EntityFrameworkCore;
    using University.Common.Infrastructure.Extensions;
    using University.Data;
    using University.Data.Models;
    using University.Services.Admin.Models.Courses;

    public class AdminCourseService : IAdminCourseService
    {
        private const int ResultInvalidId = int.MinValue;

        private readonly UniversityDbContext db;
        private readonly IMapper mapper;

        public AdminCourseService(
            UniversityDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<AdminCourseBasicServiceModel>> AllAsync()
            => await this.db
            .Courses
            .OrderBy(c => c.Name)
            .ThenByDescending(c => c.StartDate)
            .ProjectTo<AdminCourseBasicServiceModel>(this.mapper.ConfigurationProvider)
            .ToListAsync();

        public async Task<int> CreateAsync(
            string name,
            string description,
            DateTime startDate, // local time
            DateTime endDate,   // local time
            decimal price,
            string trainerId)
        {
            var trainerExists = this.db.Users.Any(u => u.Id == trainerId);
            if (!trainerExists
                || string.IsNullOrWhiteSpace(name)
                || string.IsNullOrWhiteSpace(description)
                || price < 0)
            {
                return ResultInvalidId;
            }

            var course = new Course
            {
                Name = name.Trim(),
                Description = description.Trim(),
                StartDate = startDate.ToStartDateUtc(),
                EndDate = endDate.ToEndDateUtc(),
                Price = price,
                TrainerId = trainerId
            };

            await this.db.Courses.AddAsync(course);
            var result = await this.db.SaveChangesAsync();

            return course.Id;
        }

        public async Task<AdminCourseServiceModel> GetByIdAsync(int id)
            => await this.db
            .Courses
            .Where(c => c.Id == id)
            .ProjectTo<AdminCourseServiceModel>(this.mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        public async Task<bool> RemoveAsync(int id)
        {
            var course = await this.db.Courses.FindAsync(id);
            if (course == null)
            {
                return false;
            }

            this.db.Courses.Remove(course);
            var result = await this.db.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> UpdateAsync(
            int id,
            string name,
            string description,
            DateTime startDate, // local time
            DateTime endDate, // local time
            decimal price,
            string trainerId)
        {
            var course = await this.db.Courses.FindAsync(id);
            if (course == null)
            {
                return false;
            }

            var trainerExists = this.db.Users.Any(u => u.Id == trainerId);
            if (!trainerExists
                || string.IsNullOrWhiteSpace(name)
                || string.IsNullOrWhiteSpace(description)
                || price < 0)
            {
                return false;
            }

            course.Name = name.Trim();
            course.Description = description.Trim();
            course.StartDate = startDate.ToStartDateUtc();
            course.EndDate = endDate.ToEndDateUtc();
            course.TrainerId = trainerId;
            course.Price = price;

            var result = await this.db.SaveChangesAsync();

            return result > 0;
        }
    }
}
