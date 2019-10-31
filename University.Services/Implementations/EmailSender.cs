namespace University.Services.Implementations
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.Extensions.Options;
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using University.Services.Models.EmailSender;

    public class EmailSender : IEmailSender
    {
        private readonly EmailSenderOptions options; // SendGrid

        public EmailSender(IOptions<EmailSenderOptions> optionsAccessor)
        {
            this.options = optionsAccessor.Value;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SendGridClient(this.options.SendGridApiKey);

            var from = new EmailAddress(this.options.SenderEmail, this.options.SenderName);
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, message, message);

            return client.SendEmailAsync(msg);
        }
    }
}
