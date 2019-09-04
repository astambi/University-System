namespace University.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using University.Services.Models.Resources;

    public interface IResourceService
    {
        Task<IEnumerable<ResourceServiceModel>> AllByCourseAsync(int courseId);

        Task<bool> CanBeDownloadedByUserAsync(int id, string userId);

        Task<bool> CreateAsync(int courseId, string fileName, string fileUrl);

        bool Exists(int id);

        Task<string> GetDownloadUrlAsync(int id);

        Task<bool> RemoveAsync(int id);
    }
}
