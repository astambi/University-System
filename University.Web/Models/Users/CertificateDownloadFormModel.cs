namespace University.Web.Models.Users
{
    public class CertificateDownloadFormModel
    {
        public string Id { get; set; }

        public FormActionEnum Action { get; set; } = FormActionEnum.Certificate; // Certificate or Diploma
    }
}
