namespace University.Web.Areas.Identity.Pages.Account
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;
    using University.Data;
    using University.Data.Models;

    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<ExternalLoginModel> logger)
        {
            this._signInManager = signInManager;
            this._userManager = userManager;
            this._logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string LoginProvider { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel : IValidatableObject
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            // Custom User
            [Required]
            [StringLength(DataConstants.UserUsernameMaxLength,
                ErrorMessage = DataConstants.StringMinMaxLength,
                MinimumLength = DataConstants.UserNameMinLength)]
            [Display(Name = "Username")]
            public string UserName { get; set; }

            [Required]
            [StringLength(DataConstants.UserNameMaxLength,
                ErrorMessage = DataConstants.StringMinMaxLength,
                MinimumLength = DataConstants.UserNameMinLength)]
            public string Name { get; set; }

            [Required]
            [DataType(DataType.Date)]
            public DateTime? Birthdate { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                var isFutureDate = DateTime.UtcNow.Date < this.Birthdate.Value.Date;
                if (isFutureDate)
                {
                    yield return new ValidationResult(DataConstants.UserBirthdate,
                        new[] { nameof(this.Birthdate) });
                }
            }
        }

        public IActionResult OnGetAsync() => this.RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = this.Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = this._signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? this.Url.Content("~/");
            if (remoteError != null)
            {
                this.ErrorMessage = $"Error from external provider: {remoteError}";
                return this.RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await this._signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                this.ErrorMessage = "Error loading external login information.";
                return this.RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await this._signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                this._logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return this.LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return this.RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                this.ReturnUrl = returnUrl;
                this.LoginProvider = info.LoginProvider;

                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    // Custom User
                    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                    var name = info.Principal.FindFirstValue(ClaimTypes.Name);

                    this.Input = new InputModel
                    {
                        Email = email,
                        // Custom User
                        UserName = email?.Split('@').First(), // suggest username from email
                        Name = name
                    };
                }
                return this.Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? this.Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await this._signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                this.ErrorMessage = "Error loading external login information during confirmation.";
                return this.RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (this.ModelState.IsValid)
            {
                var user = new User
                {
                    // Custom User
                    UserName = this.Input.UserName ?? this.Input.Email,
                    Email = this.Input.Email,
                    Name = this.Input.Name,
                    Birthdate = (DateTime)this.Input.Birthdate
                };

                var result = await this._userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await this._userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await this._signInManager.SignInAsync(user, isPersistent: false);
                        this._logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        return this.LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    this.ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            this.LoginProvider = info.LoginProvider;
            this.ReturnUrl = returnUrl;
            return this.Page();
        }
    }
}
