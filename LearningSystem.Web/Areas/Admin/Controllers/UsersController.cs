namespace LearningSystem.Web.Areas.Admin.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Admin;
    using LearningSystem.Web.Areas.Admin.Models.Users;
    using LearningSystem.Web.Infrastructure.Extensions;
    using LearningSystem.Web.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;

    public class UsersController : BaseAdminController
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<User> userManager;
        private readonly IAdminUserService adminUserService;

        public UsersController(
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager,
            IAdminUserService adminUserService)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.adminUserService = adminUserService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await this.adminUserService.AllAsync();

            var roles = await this.roleManager
                .Roles
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem { Text = r.Name, Value = r.Name })
                .ToListAsync();

            var model = new AdminUserListingViewModel
            {
                Users = users,
                RoleFormModel = new AdminUserRoleFormModel { Roles = roles }
            };

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RoleAddRemoveAsync(AdminUserRoleFormModel model)
        {
            var user = await this.userManager.FindByIdAsync(model.UserId);
            var roleExists = await this.roleManager.RoleExistsAsync(model.Role);

            if (user == null || !roleExists)
            {
                this.ModelState.AddModelError(string.Empty, WebConstants.InvalidIdentityOrRoleMsg);
            }

            if (!this.ModelState.IsValid)
            {
                return this.RedirectToAction(nameof(Index));
            }

            switch (model.Action)
            {
                case FormActionEnum.Add: await this.AddUserToRole(user, model.Role); break;
                case FormActionEnum.Remove: await this.RemoveUserFromRole(user, model.Role); break;
                default: break;
            }

            return this.RedirectToAction(nameof(Index));
        }

        private async Task AddUserToRole(User user, string role)
        {
            var result = await this.userManager.AddToRoleAsync(user, role);
            if (result.Succeeded)
            {
                this.TempData.AddSuccessMessage(string.Format(WebConstants.UserAddedToRoleMsg, user.UserName, role));
            }
            else
            {
                this.TempData.AddInfoMessages(result);
            }
        }

        private async Task RemoveUserFromRole(User user, string role)
        {
            var result = await this.userManager.RemoveFromRoleAsync(user, role);
            if (result.Succeeded)
            {
                this.TempData.AddSuccessMessage(string.Format(WebConstants.UserRemovedFromRoleMsg, user.UserName, role));
            }
            else
            {
                this.TempData.AddInfoMessages(result);
            }
        }
    }
}