namespace University.Services.Models.EmailSender
{
    public class EmailSenderOptions
    {
        public string SendGridApiKey { get; set; }

        public string SenderEmail { get; set; } // any email

        public string SenderName { get; set; } // any name
    }
}
