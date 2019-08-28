namespace University.Web.Areas.Identity.Pages.Account
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;
    using University.Data.Models;
    using University.Services;
    using University.Web.Infrastructure.Extensions;

    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IShoppingCartManager shoppingCartManager;

        public LogoutModel(
            SignInManager<User> signInManager,
            ILogger<LogoutModel> logger,
            IShoppingCartManager shoppingCartManager)
        {
            this._signInManager = signInManager;
            this._logger = logger;
            this.shoppingCartManager = shoppingCartManager;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await this._signInManager.SignOutAsync();
            this._logger.LogInformation("User logged out.");

            // Empty shopping cart on logout
            var shoppingCartId = this.HttpContext.Session.GetOrSetShoppingCartId();
            this.shoppingCartManager.EmptyCart(shoppingCartId);

            if (returnUrl != null)
            {
                return this.LocalRedirect(returnUrl);
            }
            else
            {
                return this.Page();
            }
        }
    }
}