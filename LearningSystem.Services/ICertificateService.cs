namespace LearningSystem.Services
{
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.Certificates;

    public interface ICertificateService
    {
        Task<bool> CreateAsync(string trainerId, int courseId, string studentId, Grade grade);

        Task<CertificateServiceModel> DownloadAsync(string certificateId);

        bool IsGradeEligibleForCertificate(Grade? grade);
    }
}
