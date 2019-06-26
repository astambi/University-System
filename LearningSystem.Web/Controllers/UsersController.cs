namespace LearningSystem.Web.Controllers
{
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services;
    using LearningSystem.Web.Infrastructure.Extensions;
    using LearningSystem.Web.Models.Users;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly IUserService userService;

        public UsersController(
            UserManager<User> userManager,
            IUserService userService)
        {
            this.userManager = userManager;
            this.userService = userService;
        }

        public async Task<IActionResult> Profile()
        {
            var user = await this.userManager.GetUserAsync(this.User);
            if (user == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(HomeController.Index));
            }

            var profile = await this.userService.GetUserProfileAsync(user.Id);

            var roles = await this.userManager.GetRolesAsync(user);

            var model = new UserProfileViewModel
            {
                User = profile.User,
                Courses = profile.Courses,
                Roles = roles
            };

            return this.View(model);
        }
    }
}
