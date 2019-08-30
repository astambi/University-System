namespace University.Data
{
    public class DataConstants
    {
        public const string FileMaxLength = "The {0} must be at max {1} bytes.";

        public const string StringMaxLength = "The {0} must be at max {1} characters long.";
        public const string StringMinMaxLength = "The {0} must be at least {2} and at max {1} characters long.";

        public const string NegativeNumber = "The {0} cannot be a negative number.";
        public const string RangeMinMaxValues = "The {0} must be between {2} and {1} incl.";

        public const int ArticleTitleMinLength = 3;
        public const int ArticleTitleMaxLength = 100;
        public const int ArticleContentMinLength = 3;
        public const int ArticleContentMaxLength = 15000;
        public const string ArticlePublishDate = "The Publish date cannot be in the future.";

        public const int CourseNameMaxLength = 50;
        public const int CourseDescriptionMaxLength = 2000;
        public const string CourseEndDate = "The End date cannot be before the Start date.";
        public const string CourseStartDate = "The Start date cannot be in the past.";

        public const int BytesInMb = 1024 * 1024;

        public const int ContentTypeMaxLength = 100;

        public const int FileMaxLengthInMb = 2;
        public const int FileMaxLengthInBytes = FileMaxLengthInMb * BytesInMb;
        public const string FileType = "zip";

        public const int GradeBgCertificateMinValue = 5;
        public const int GradeBgMinValue = 2;
        public const int GradeBgMaxValue = 6;

        public const int InvoiceIdMaxLength = 50;

        public const int ResourceFileMaxLengthInMb = 7;
        public const int ResourceMaxLengthInBytes = ResourceFileMaxLengthInMb * BytesInMb;
        public const int ResourceNameMinLength = 10;
        public const int ResourceNameMaxLength = 100;

        public const int RoleNameMaxLength = 50;

        public const int UserNameMinLength = 2;
        public const int UserNameMaxLength = 100;
        public const int UserUsernameMaxLength = 50;
        public const string UserBirthdate = "The Birthdate cannot be in the future.";
    }
}
