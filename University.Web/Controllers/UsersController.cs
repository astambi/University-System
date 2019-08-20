namespace University.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using University.Data.Models;
    using University.Services;
    using University.Web.Infrastructure.Extensions;
    using University.Web.Models.Users;

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

        public async Task<IActionResult> Courses()
        {
            var user = await this.userManager.GetUserAsync(this.User);
            if (user == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(HomeController.Index));
            }

            var courses = await this.userService.GetCoursesAsync(user.Id);

            return this.View(courses);
        }

        public async Task<IActionResult> Certificates()
        {
            var user = await this.userManager.GetUserAsync(this.User);
            if (user == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(HomeController.Index));
            }

            var certificates = await this.userService.GetCertificatesAsync(user.Id);

            return this.View(certificates);
        }

        public async Task<IActionResult> Exams()
        {
            var user = await this.userManager.GetUserAsync(this.User);
            if (user == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(HomeController.Index));
            }

            var exams = await this.userService.GetExamsAsync(user.Id);

            return this.View(exams);
        }

        public async Task<IActionResult> Resources()
        {
            var user = await this.userManager.GetUserAsync(this.User);
            if (user == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(HomeController.Index));
            }

            var resources = await this.userService.GetResourcesAsync(user.Id);

            return this.View(resources);
        }

        public async Task<IActionResult> Profile()
        {
            var user = await this.userManager.GetUserAsync(this.User);
            if (user == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(HomeController.Index));
            }

            var userProfile = await this.userService.GetProfileAsync(user.Id);
            var roles = await this.GetRolesFriendlyNames(user);

            var model = new UserProfileViewModel { User = userProfile, Roles = roles };

            return this.View(model);
        }

        private async Task<IEnumerable<string>> GetRolesFriendlyNames(User user)
            => (await this.userManager.GetRolesAsync(user))
            .Select(r => r.ToFriendlyName())
            .OrderBy(r => r);
    }
}
