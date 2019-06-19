namespace LearningSystem.Services.Admin.Implementations
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Admin.Models;

    public class AdminCourseService : IAdminCourseService
    {
        private readonly LearningSystemDbContext db;
        private readonly IMapper mapper;

        public AdminCourseService(LearningSystemDbContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task Create(
            string name,
            string description,
            DateTime startDate,
            DateTime endDate,
            string trainerId)
        {
            var trainerExists = this.db.Users.Any(u => u.Id == trainerId);
            if (!trainerExists)
            {
                return;
            }

            var course = new Course
            {
                Name = name,
                Description = description,
                StartDate = startDate,
                EndDate = endDate,
                TrainerId = trainerId
            };

            await this.db.Courses.AddAsync(course);
            await this.db.SaveChangesAsync();
        }

        public CourseEditServiceModel GetById(int id)
            => this.db.Courses
            .Where(c => c.Id == id)
            .Select(c => this.mapper.Map<CourseEditServiceModel>(c))
            .FirstOrDefault();

        public async Task UpdateAsync(
            int id,
            string name,
            string description,
            DateTime startDate,
            DateTime endDate,
            string trainerId)
        {
            var course = await this.db.Courses.FindAsync(id);
            if (course == null)
            {
                return;
            }

            var trainerExists = this.db.Users.Any(u => u.Id == trainerId);
            if (!trainerExists)
            {
                return;
            }

            course.Name = name;
            course.Description = description;
            course.StartDate = startDate;
            course.EndDate = endDate;
            course.TrainerId = trainerId;

            await this.db.SaveChangesAsync();
        }
    }
}
