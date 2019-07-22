namespace LearningSystem.Services
{
    using System.Threading.Tasks;
    using LearningSystem.Services.Models.Resources;

    public interface IResourceService
    {
        Task<bool> CreateAsync(int courseId, string fileName, string contentType, byte[] fileBytes);

        Task<ResourceDownloadServiceModel> DownloadAsync(int id);

        bool Exists(int id);

        Task<bool> RemoveAsync(int id);
    }
}
