namespace University.Web.Areas.Identity.Pages.Account.Manage
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.WebUtilities;
    using University.Data;
    using University.Data.Models;
    using University.Services;
    using University.Web.Infrastructure.Extensions;

    public partial class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IUserService userService;

        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender,
            IUserService userService)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._emailSender = emailSender;
            this.userService = userService;
        }

        public string Username { get; set; }

        //Custom User Data
        public string Name { get; set; }

        //Custom User Data
        [Required]
        [DataType(DataType.Date)]
        public DateTime Birthdate { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel : IValidatableObject
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            // Custom User data
            [Required]
            [StringLength(DataConstants.UserNameMaxLength,
                ErrorMessage = DataConstants.StringMinMaxLength,
                MinimumLength = DataConstants.UserNameMinLength)]
            public string Name { get; set; }

            [Required]
            [DataType(DataType.Date)]
            public DateTime Birthdate { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                var isFutureDate = DateTime.UtcNow.Date < this.Birthdate.Date;
                if (isFutureDate)
                {
                    yield return new ValidationResult(DataConstants.UserBirthdate,
                        new[] { nameof(this.Birthdate) });
                }
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToPage();
                //return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var userName = await this._userManager.GetUserNameAsync(user);
            var email = await this._userManager.GetEmailAsync(user);
            var phoneNumber = await this._userManager.GetPhoneNumberAsync(user);

            this.Username = userName;

            // Custom User Data
            var profileToEdit = await this.userService.GetProfileToEditAsync(user.Id);

            this.Input = new InputModel
            {
                Email = email,
                PhoneNumber = phoneNumber,
                //Custom User Data
                Name = profileToEdit?.Name,
                Birthdate = profileToEdit?.Birthdate.Date ?? DateTime.UtcNow
            };

            this.IsEmailConfirmed = await this._userManager.IsEmailConfirmedAsync(user);

            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!this.ModelState.IsValid)
            {
                this.Username = (await this._userManager.GetUserAsync(this.User))?.UserName;
                return this.Page();
            }

            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToPage();
                //return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var email = await this._userManager.GetEmailAsync(user);
            if (this.Input.Email != email)
            {
                var setEmailResult = await this._userManager.SetEmailAsync(user, this.Input.Email);
                if (!setEmailResult.Succeeded)
                {
                    this.TempData.AddErrorMessages(setEmailResult);
                    return this.RedirectToPage();
                    //var userId = await this._userManager.GetUserIdAsync(user);
                    //throw new InvalidOperationException($"Unexpected error occurred setting email for user with ID '{userId}'.");
                }
            }

            var phoneNumber = await this._userManager.GetPhoneNumberAsync(user);
            if (this.Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await this._userManager.SetPhoneNumberAsync(user, this.Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    this.TempData.AddErrorMessages(setPhoneResult);
                    return this.RedirectToPage();
                    //var userId = await this._userManager.GetUserIdAsync(user);
                    //throw new InvalidOperationException($"Unexpected error occurred setting phone number for user with ID '{userId}'.");
                }
            }

            // Update Custom User Data
            if (user.Name != this.Input.Name
                || user.Birthdate != this.Input.Birthdate)
            {
                var success = await this.userService.UpdateProfileAsync(user.Id, this.Input.Name, this.Input.Birthdate);
            }

            this.StatusMessage = "Your profile has been updated";
            await this._signInManager.RefreshSignInAsync(user);
            return this.RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToPage();
                //return this.NotFound($"Unable to load user with ID '{this._userManager.GetUserId(this.User)}'.");
            }

            var userId = await this._userManager.GetUserIdAsync(user);
            var email = await this._userManager.GetEmailAsync(user);

            var code = await this._userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)); // NB!

            var callbackUrl = this.Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: this.Request.Scheme);

            await this._emailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            this.StatusMessage = "Verification email sent. Please check your email.";

            return this.RedirectToPage();
        }
    }
}
