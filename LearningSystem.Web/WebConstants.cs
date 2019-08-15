namespace LearningSystem.Web
{
    using System.Collections.Generic;
    using LearningSystem.Web.Models;

    public class WebConstants
    {
        // Roles
        public const string AdministratorRole = "Administrator";
        public const string BlogAuthorRole = "BlogAuthor";
        public const string TrainerRole = "Trainer";
        public const string StudentRole = "Student";
        // Student Role => default role for registered users

        // Admin
        public const string AdminEmail = "admin@admin.com";
        public const string AdminUsername = "admin";

        // Secret Manager Keys
        public const string AdminPassword = "AdminPassword";
        public const string AuthFacebookAppId = "Authentication:Facebook:AppId";
        public const string AuthFacebookAppSecret = "Authentication:Facebook:AppSecret";
        public const string AuthGoogleClientId = "Authentication:Google:ClientId";
        public const string AuthGoogleClientSecret = "Authentication:Google:ClientSecret";

        // TempData Keys
        public const string TempDataErrorMessageKey = "ErrorMessage";
        public const string TempDataInfoMessageKey = "InfoMessage";
        public const string TempDataSuccessMessageKey = "SuccessMessage";

        // Areas
        public const string AdminArea = "Admin";
        public const string BlogArea = "Blog";

        // Controllers
        public const string ArticlesController = "Articles";
        public const string CertificatesController = "Certificates";
        public const string CoursesController = "Courses";
        public const string ExamsController = "Exams";
        public const string HomeController = "Home";
        public const string OrdersController = "Orders";
        public const string ResourcesController = "Resources";
        public const string ShoppingCartController = "ShoppingCart";
        public const string TrainersController = "Trainers";
        public const string UsersController = "Users";

        // Actions
        public const string Index = "Index";

        // Pagination
        public const int PageSize = 6;

        // Session
        public const string ShoppingCartKey = "ShoppingCartId";

        // Routing
        public const string Id = "id";
        public const string WithId = "{id}";

        // ContentType
        public const string ApplicationPdf = "application/pdf";
        public const string ApplicationZip = "application/zip";
        public const string CertificateFileName = "Certificate.pdf";

        // Bootstrap Styles
        public const string DangerStyle = "danger";
        public const string ErrorStyle = "danger";
        public const string InfoStyle = "info";
        public const string PrimaryStyle = "primary";
        public const string SuccessStyle = "success";
        public const string WarningStyle = "warning";

        public const string OutlineStyle = "outline-";
        public const string OutlineDangerStyle = OutlineStyle + DangerStyle;
        public const string OutlineErrorStyle = OutlineStyle + ErrorStyle;
        public const string OutlineInfoStyle = OutlineStyle + InfoStyle;
        public const string OutlinePrimaryStyle = OutlineStyle + PrimaryStyle;
        public const string OutlineSuccessStyle = OutlineStyle + SuccessStyle;
        public const string OutlineWarningStyle = OutlineStyle + WarningStyle;

        // Notifications
        public const string ArticleNotFoundMsg = "Article not found.";
        public const string ArticlePublishedMsg = "Article published successfully.";

        public const string CertificateIssuedMsg = "Course certificate issued successfully.";
        public const string CertificateNotFoundMsg = "Certificate not found.";
        public const string CertificateDownloadInProgressMsg = "Certificate download in progress. This could take a few more seconds.";
        public const string CertificateDownloadSuccessMsg = "Certificate downloaded successfully.";

        public const string CourseAddedToShoppingCartSuccessMsg = "Course added to shopping cart successfully.";
        public const string CourseCreateErrorMsg = "Course not created.";
        public const string CourseCreateSuccessMsg = "Course created successfully.";
        public const string CourseDeleteSuccessMsg = "Course deleted successfully.";
        public const string CourseEnrollmentCancellationErrorMsg = "Course enrollment cannot be cancelled after start date.";
        public const string CourseEnrollmentCancellationSuccessMsg = "Course enrollment cancelled successfully.";
        public const string CourseEnrollmentClosedMsg = "Course is closed for enrollment after start date.";
        public const string CourseEnrollmentErrorMsg = "Course enrollment error.";
        public const string CourseEnrollmentOpenMsg = "Course is open for enrollment for {0}."; // days/hours
        public const string CourseEnrollmentSuccessMsg = "Enrolled in course successfully.";
        public const string CoursesEnrollmentSuccessMsg = "Enrolled in courses successfully.";
        public const string CourseHasNotEndedMsg = "Course has not ended.";
        public const string CourseInvalidMsg = "Invalid course.";
        public const string CourseNotFoundMsg = "Course not found.";
        public const string CourseRemovedFromShoppingCartSuccessMsg = "Course removed from shopping cart successfully.";
        public const string CourseUpdateErrorMsg = "Course not updated.";
        public const string CourseUpdateSuccessMsg = "Course updated successfully.";

        public const string Disable2faErrorMsg = "Cannot disable 2FA for user.";
        public const string ExternalLoginInfoErrorMsg = "Unable to load external login info.";

        public const string FormErrorsMsg = "Review the form for errors.";

        public const string ExamAssessedMsg = "Exam was assessed successfully.";
        public const string ExamAssessmentErrorMsg = "Exam was not assessed.";
        public const string ExamSubmittedMsg = "Exam file submitted successfully.";

        public const string FileFormatInvalidMsg = "File format should be {0}.";
        public const string FileSubmittionDateMsg = "File can only be submitted on exam date.";
        public const string FileNotSubmittedMsg = "No file was submitted.";
        public const string FileEmptyErrorMsg = "File cannot be empty.";
        public const string FileSizeInvalidMsg = "File size should not exceed {0} MB.";
        public const string FileInvalidMsg = "Invalid file size.";

        public const string InvalidIdentityOrRoleMsg = "Invalid identity or role.";
        public const string InvalidUserMsg = "Invalid user.";

        public const string InvoiceNotFoundMsg = "Invoice not found.";

        public const string OrderCoursesEnrollmentCancellationSuccessMsg = "Enrollment in all order courses cancelled successfully.";
        public const string OrderCreatedSuccessMsg = "Payment success.";
        public const string OrderDeletedSuccessMsg = "Order deleted successfully.";
        public const string OrderDeletedErrorMsg = "Order not deleted.";
        public const string OrderNotFoundMsg = "Order not found.";

        public const string PaymentMethodInvalidMsg = "Invalid payment method.";
        public const string PaymentErrorMsg = "Payment error.";

        public const string ShoppingCartItemsMismatchMsg = "Shopping cart items mismatch.";
        public const string ShoppingCartEmptyMsg = "Shopping cart is empty.";
        public const string ShoppingCartClearedMsg = "Shopping cart cleared successfully.";

        public const string ResourceCreatedMsg = "Resource created successfully.";
        public const string ResourceDeletedMsg = "Resource deleted successfully.";
        public const string ResourceDownloadErrorMsg = "Resource download error.";
        public const string ResourceDownloadUnauthorizedMsg = "Resource download not allowed.";
        public const string ResourceFileUploadErrorMsg = "Resource file upload error.";
        public const string ResourceNotDeletedMsg = "Resource not deleted.";
        public const string ResourceNotFoundMsg = "Resource not found.";

        public const string StudentAssessmentErrorMsg = "Student assessment error.";
        public const string StudentHasNotSubmittedExamMsg = "Student has not submitted an exam.";
        public const string StudentNotEnrolledInCourseMsg = "Student not enrolled in this course.";

        public const string SearchByArticleTitleOrContent = "Search by title or content";
        public const string SearchByCourseName = "Search by course name";
        public const string SearchByUserName = "Search by name";

        public const string NotTrainerForCourseMsg = "Not authorized as trainer for this course.";
        public const string TrainersAssessPerformanceAfterCourseEndsMsg = "Trainers can assess students' course performance only when the course is over.";

        public const string UserAddedToRoleMsg = "User {0} added to role {1}.";
        public const string UserDeleteErrorMsg = "User cannot be deleted. Delete user's blog articles & remove user as trainer from all courses first.";
        public const string UserEnrolledInCourseAlreadyMsg = "User already enrolled in this course.";
        public const string UserNotEnrolledInCourseMsg = "User not enrolled in this course.";
        public const string UserRemovedFromRoleMsg = "User {0} removed from role {1}.";

        public static Dictionary<string, string> Styles;

        static WebConstants()
        {
            Styles = new Dictionary<string, string>
            {
                [TempDataErrorMessageKey] = ErrorStyle,
                [TempDataInfoMessageKey] = InfoStyle,
                [TempDataSuccessMessageKey] = SuccessStyle,
                [FormActionEnum.Add.ToString()] = OutlineSuccessStyle,
                [FormActionEnum.CancelEnrollment.ToString()] = OutlineDangerStyle,
                [FormActionEnum.Certificate.ToString()] = InfoStyle,
                [FormActionEnum.Create.ToString()] = OutlineSuccessStyle,
                [FormActionEnum.Default.ToString()] = OutlinePrimaryStyle,
                [FormActionEnum.Delete.ToString()] = OutlineDangerStyle,
                [FormActionEnum.Download.ToString()] = InfoStyle,
                [FormActionEnum.Edit.ToString()] = OutlineWarningStyle,
                [FormActionEnum.Enroll.ToString()] = SuccessStyle,
                [FormActionEnum.Grade.ToString()] = OutlineSuccessStyle,
                [FormActionEnum.Remove.ToString()] = OutlineDangerStyle,
                [FormActionEnum.Search.ToString()] = OutlineInfoStyle,
                [FormActionEnum.Upload.ToString()] = OutlineSuccessStyle,
            };
        }

    }
}
