namespace University.Web.Areas.Identity.Pages.Account
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;
    using University.Data;
    using University.Data.Models;

    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._logger = logger;
            this._emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel : IValidatableObject
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
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

            [Required]
            [StringLength(100,
                ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password",
                ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

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

        public void OnGet(string returnUrl = null) => this.ReturnUrl = returnUrl;

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? this.Url.Content("~/");
            if (this.ModelState.IsValid)
            {
                // Custom User
                var user = new User
                {
                    UserName = this.Input.UserName,
                    Email = this.Input.Email,
                    Name = this.Input.Name,
                    Birthdate = this.Input.Birthdate.Value
                };

                var result = await this._userManager.CreateAsync(user, this.Input.Password);
                if (result.Succeeded)
                {
                    this._logger.LogInformation("User created a new account with password.");

                    var code = await this._userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)); // NB!

                    var callbackUrl = this.Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: this.Request.Scheme);

                    await this._emailSender.SendEmailAsync(
                        this.Input.Email,
                        "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    await this._signInManager.SignInAsync(user, isPersistent: false);
                    return this.LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    this.ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return this.Page();
        }
    }
}
