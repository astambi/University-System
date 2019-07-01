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
    using Microsoft.AspNetCore.Routing;
    using SelectPdf;

    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly IUserService userService;
        private readonly IPdfService pdfService;

        public UsersController(
            UserManager<User> userManager,
            IUserService userService,
            IPdfService pdfService)
        {
            this.userManager = userManager;
            this.userService = userService;
            this.pdfService = pdfService;
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

        [AllowAnonymous]
        [Route(nameof(Certificate) + "/{id}")]
        public async Task<IActionResult> Certificate(string id) // read by pdfConverter
        {
            var certificate = await this.userService.GetCertificateDataAsync(id);
            if (certificate == null)
            {
                this.TempData.AddErrorMessage(WebConstants.CertificateNotFoundMsg);
                return this.RedirectToAction(nameof(HomeController.Index));
            }

            certificate.DownloadUrl = this.HttpContext.Request.GetRequestUrl();

            return this.View(certificate);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route(nameof(Certificate) + "/{id}")]
        public IActionResult DownloadCertificate(string id)
        {
            var downloadUrl = this.HttpContext.Request.GetRequestUrl();

            var pdf = this.pdfService.ConvertToPdf(downloadUrl);
            if (pdf == null)
            {
                this.TempData.AddErrorMessage(WebConstants.CertificateNotFoundMsg);
                return this.RedirectToAction(nameof(HomeController.Index));
            }

            return this.File(pdf, "application/pdf", "Certificate.pdf");
        }
    }
}
