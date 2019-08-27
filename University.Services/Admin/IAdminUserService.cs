namespace University.Services.Admin
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using University.Services.Admin.Models.Users;

    public interface IAdminUserService
    {
        Task<IEnumerable<AdminUserListingServiceModel>> AllAsync();
    }
}
