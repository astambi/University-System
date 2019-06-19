namespace LearningSystem.Services.Implementations
{
    using System.Linq;
    using AutoMapper;
    using LearningSystem.Data;

    public class CourseService : ICourseService
    {
        private readonly LearningSystemDbContext db;
        private readonly IMapper mapper;

        public CourseService(
            LearningSystemDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public bool Exists(int id)
            => this.db.Courses.Any(c => c.Id == id);
    }
}
