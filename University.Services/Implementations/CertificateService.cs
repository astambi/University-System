namespace University.Services.Implementations
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using University.Data;
    using University.Data.Models;
    using University.Services.Models.Certificates;
    using Microsoft.EntityFrameworkCore;

    public class CertificateService : ICertificateService
    {
        private readonly UniversityDbContext db;
        private readonly IMapper mapper;

        public CertificateService(
            UniversityDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<bool> CreateAsync(string trainerId, int courseId, string studentId, Grade grade)
        {
            var isCourseTrainer = await this.db
                .Courses
                .Where(c => c.Id == courseId)
                .Where(c => c.TrainerId == trainerId)
                .AnyAsync();

            var isUserEnrolledInCourse = await this.db
                .Courses
                .AnyAsync(c => c.Students.Any(sc => sc.StudentId == studentId));

            if (!(isCourseTrainer
                && isUserEnrolledInCourse
                && this.IsGradeEligibleForCertificate(grade)))
            {
                return false;
            }

            var prevBestCertificate = await this.db
                .Certificates
                .Where(c => c.CourseId == courseId)
                .Where(c => c.StudentId == studentId)
                .OrderBy(c => c.Grade)
                .FirstOrDefaultAsync();

            var canUpgradeCertificate =
                prevBestCertificate == null // no prev certificate
                || grade < prevBestCertificate.Grade; // Enum Grade value smaller is better (A = 0, B = 1, etc.)

            if (!canUpgradeCertificate)
            {
                return false;
            }

            var certificate = new Certificate
            {
                Id = Guid.NewGuid().ToString().Replace("-", string.Empty),
                StudentId = studentId,
                CourseId = courseId,
                Grade = grade,
                IssueDate = DateTime.UtcNow
            };

            await this.db.Certificates.AddAsync(certificate);
            await this.db.SaveChangesAsync();

            return true;
        }

        public async Task<CertificateServiceModel> DownloadAsync(string certificateId)
            => await this.mapper
            .ProjectTo<CertificateServiceModel>(
                this.db
                .Certificates
                .Where(c => c.Id == certificateId))
            .FirstOrDefaultAsync();

        public bool IsGradeEligibleForCertificate(Grade? grade)
            => grade != null
            && Grade.A <= grade && grade <= Grade.C;
    }
}
