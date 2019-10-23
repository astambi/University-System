namespace University.Web.Areas.Admin.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using University.Data.Models;
    using University.Services.Admin;
    using University.Services.Admin.Models.Users;
    using University.Web.Areas.Admin.Models.Users;
    using University.Web.Infrastructure.Extensions;
    using University.Web.Models;

    public class UsersController : BaseAdminController
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<User> userManager;
        private readonly IAdminUserService adminUserService;
        private readonly IMapper mapper;

        public UsersController(
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager,
            IAdminUserService adminUserService,
            IMapper mapper)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.adminUserService = adminUserService;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var users = await this.adminUserService.AllAsync();
            var rolesSelectListItems = await this.GetRoles();
            var rolesWithUsers = await this.GetUsersInRoleAsync(rolesSelectListItems.Select(r => r.Value));

            var model = new AdminUserListingViewModel
            {
                RolesWithUsersInRole = rolesWithUsers,
                Users = users,
                Roles = rolesSelectListItems
            };

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AdminRoleFormModel model)
        {
            if (!this.ModelState.IsValid)
            {
                this.TempData.AddErrorMessage(WebConstants.RoleNameInvalidErrorMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var roleName = model.Name.Trim().Replace(" ", string.Empty);

            var roleExists = await this.roleManager.RoleExistsAsync(roleName);
            if (roleExists)
            {
                this.TempData.AddErrorMessage(WebConstants.RoleExistsErrorMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var role = await this.roleManager.CreateAsync(new IdentityRole { Name = roleName });
            if (role == null)
            {
                this.TempData.AddErrorMessage(WebConstants.RoleCreateErrorMsg);
            }
            else
            {
                this.TempData.AddSuccessMessage(WebConstants.RoleCreateSuccessMsg);
            }

            return this.RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(AdminRoleFormModel model)
        {
            var role = this.roleManager.Roles.FirstOrDefault(r => r.Name == model.Name);
            if (role == null)
            {
                this.TempData.AddErrorMessage(WebConstants.RoleNotFoundErrorMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var identityResult = await this.roleManager.DeleteAsync(role);
            if (!identityResult.Succeeded)
            {
                this.TempData.AddErrorMessage(WebConstants.RoleDeleteErrorMsg);
            }
            else
            {
                this.TempData.AddSuccessMessage(WebConstants.RoleDeleteSuccessMsg);
            }

            return this.RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(AdminUserRoleFormModel model)
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

        private async Task<List<SelectListItem>> GetRoles()
            => await this.roleManager
            .Roles
            .OrderBy(r => r.Name)
            .Select(r => new SelectListItem { Text = r.Name, Value = r.Name })
            .ToListAsync();

        private async Task<IEnumerable<RoleWithUsersViewModel>> GetUsersInRoleAsync(IEnumerable<string> roles)
        {
            var rolesWithUsers = new List<RoleWithUsersViewModel>();
            foreach (var role in roles)
            {
                rolesWithUsers.Add(new RoleWithUsersViewModel
                {
                    Role = role,
                    UsersInRole = (await this.userManager.GetUsersInRoleAsync(role))
                        .AsQueryable()
                        .ProjectTo<AdminUserListingServiceModel>(this.mapper.ConfigurationProvider)
                        .OrderBy(u => u.Name)
                        .ToList()
                });
            }

            return rolesWithUsers;
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