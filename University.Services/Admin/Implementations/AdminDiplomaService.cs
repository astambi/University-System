namespace University.Services.Admin.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using University.Data;
    using University.Data.Models;

    public class AdminDiplomaService : IAdminDiplomaService
    {
        private readonly UniversityDbContext db;
        private readonly IMapper mapper;

        public AdminDiplomaService(
            UniversityDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<bool> CreateAsync(int curriculumId, string studentId)
        {
            var isEligibleForDiploma = await this.IsEligibleForDiplomaAsync(curriculumId, studentId);
            var diplomaExists = await this.ExistsForCurriculumStudentAsync(curriculumId, studentId);

            if (!isEligibleForDiploma
                || diplomaExists)
            {
                return false;
            }

            await this.db.Diplomas.AddAsync(new Diploma
            {
                CurriculumId = curriculumId,
                StudentId = studentId,
                IssueDate = DateTime.UtcNow
            });

            var result = await this.db.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> ExistsAsync(string id)
            => await this.db.Diplomas.AnyAsync(d => d.Id == id);

        public async Task<bool> ExistsForCurriculumStudentAsync(int curriculumId, string studentId)
            => await this.db
            .Diplomas
            .AnyAsync(d => d.StudentId == studentId && d.CurriculumId == curriculumId);

        public async Task<bool> IsEligibleForDiplomaAsync(int curriculumId, string studentId)
        {
            var curriculumExists = await this.db.Curriculums.AnyAsync(c => c.Id == curriculumId);
            var studentExists = await this.db.Users.AnyAsync(u => u.Id == studentId);

            if (!curriculumExists || !studentExists)
            {
                return false;
            }

            var curriculumCourses = await this.db
                .Curriculums
                .Where(c => c.Id == curriculumId)
                .SelectMany(c => c.Courses)
                .Select(c => c.CourseId)
                .ToListAsync();

            var coursesWithCertificates = await this.db
                .Certificates
                .Where(c => c.StudentId == studentId)
                .Select(c => c.CourseId)
                .ToListAsync();

            var coursesPassed = new HashSet<int>(coursesWithCertificates);

            foreach (var curriculumCourse in curriculumCourses)
            {
                if (!coursesPassed.Contains(curriculumCourse))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> RemoveAsync(string id)
        {
            var diploma = await this.db.Diplomas.FindAsync(id);
            if (diploma == null)
            {
                return false;
            }

            this.db.Diplomas.Remove(diploma);
            var result = await this.db.SaveChangesAsync();

            return result > 0;
        }
    }
}