namespace University.Web.Areas.Identity.Pages.Account.Manage
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using University.Data.Models;
    using University.Web.Infrastructure.Extensions;

    public class ExternalLoginsModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public ExternalLoginsModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
        }

        public IList<UserLoginInfo> CurrentLogins { get; set; }

        public IList<AuthenticationScheme> OtherLogins { get; set; }

        public bool ShowRemoveButton { get; set; }

        [TempData]
        public string ErrorMessage { get; set; } // error msg for invalid external login

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            this.CurrentLogins = await this._userManager.GetLoginsAsync(user);
            this.OtherLogins = (await this._signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => this.CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();
            this.ShowRemoveButton = user.PasswordHash != null || this.CurrentLogins.Count > 1;
            return this.Page();
        }

        public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey)
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToPage();
                //return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var result = await this._userManager.RemoveLoginAsync(user, loginProvider, providerKey);
            if (!result.Succeeded)
            {
                this.TempData.AddErrorMessages(result);
                return this.RedirectToPage();
                //var userId = await this._userManager.GetUserIdAsync(user);
                //throw new InvalidOperationException($"Unexpected error occurred removing external login for user with ID '{userId}'.");
            }

            await this._signInManager.RefreshSignInAsync(user);
            this.StatusMessage = "The external login was removed.";
            return this.RedirectToPage();
        }

        public async Task<IActionResult> OnPostLinkLoginAsync(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await this.HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = this.Url.Page("./ExternalLogins", pageHandler: "LinkLoginCallback");
            var properties = this._signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, this._userManager.GetUserId(this.User));
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToPage();
                //return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var info = await this._signInManager.GetExternalLoginInfoAsync(await this._userManager.GetUserIdAsync(user));
            if (info == null)
            {
                this.TempData.AddErrorMessage(WebConstants.ExternalLoginInfoErrorMsg);
                return this.RedirectToPage();
                //throw new InvalidOperationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");
            }

            var result = await this._userManager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                // Error Message for adding external provider login
                this.TempData.AddErrorMessages(result);
                return this.RedirectToPage();

                //throw new InvalidOperationException($"Unexpected error occurred adding external login for user with ID '{user.Id}'.");
            }

            // Clear the existing external cookie to ensure a clean login process
            await this.HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            this.StatusMessage = "The external login was added.";
            return this.RedirectToPage();
        }
    }
}
