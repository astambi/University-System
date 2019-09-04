namespace University.Web
{
    using System.Collections.Generic;
    using University.Web.Models;

    public class WebConstants
    {
        // Project
        public const string University = "Online University";

        // Roles
        public const string AdministratorRole = "Administrator";
        public const string BloggerRole = "Blogger";
        public const string TrainerRole = "Trainer";
        // Student Role => default role for registered users

        // Admin
        public const string AdminEmail = "admin@admin.com";
        public const string AdminUsername = "admin";

        // Secret Manager Keys
        public const string AdminPassword = "AdminPassword";

        public const string AuthCloudinaryCloudName = "Authentication:Cloudinary:CloudName";
        public const string AuthCloudinaryApiKey = "Authentication:Cloudinary:ApiKey";
        public const string AuthCloudinaryApiSecret = "Authentication:Cloudinary:ApiSecret";

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
        public const string CurriculumsController = "Curriculums";
        public const string DiplomasController = "Diplomas";
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

        // Cache
        public const string CacheCandidatesKey = "Cache_Curriculum_Candidates_Key-";
        public const string CacheGraduatesKey = "Cache_Curriculum_Graduates_Key-";
        public const int CacheExpirationInMinutes = 15;

        // Routing
        public const string Id = "id";
        public const string WithId = "{id}";
        public const string WithOptionalTitle = "{title?}";

        // ContentType
        public const string ApplicationPdf = "application/pdf";

        // Azure deployment
        public const string AzureWeb = "azurewebsites";

        // Cloudinary
        public const string CloudProjectFolder = "UniversitySystem";
        public const string CloudCertificatesFolder = "Certificates";
        public const string CloudExamsFolder = "Exams";
        public const string CloudResourcesFolder = "Resources";

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
        public const string ArticleNotFoundForAuthorMsg = "Article does not belong to this author.";
        public const string ArticleCreateSuccessMsg = "Article published successfully.";
        public const string ArticleCreateErrorMsg = "Could not create article.";
        public const string ArticleDeleteSuccessMsg = "Article deleted successfully.";
        public const string ArticleDeleteErrorMsg = "Could not delete article.";
        public const string ArticleUpdateSuccessMsg = "Article updated successfully.";
        public const string ArticleUpdateErrorMsg = "Article was not updated.";

        public const string CertificateIssuedMsg = "Course certificate issued successfully.";
        public const string CertificateNotFoundMsg = "Certificate not found.";
        public const string CertificateDownloadInProgressMsg = "Certificate download in progress. This could take a few more seconds.";
        public const string CertificateDeletedErrorMsg = "Could not delete certificate.";
        public const string CertificateDeletedSuccessMsg = "Certificate deleted successfully.";
        public const string CertificateDownloadSuccessMsg = "Certificate downloaded successfully.";

        public const string CourseAddedToCurriculumErrorMsg = "Could not add course to curriculum.";
        public const string CourseAddedToCurriculumSuccessMsg = "Course added to curriculum successfully.";
        public const string CourseRemoveFromCurriculumErrorMsg = "Could not remove course from curriculum.";
        public const string CourseRemoveFromCurriculumSuccessMsg = "Course removed from curriculum successfully.";

        public const string CourseAddedToShoppingCartSuccessMsg = "Course added to shopping cart successfully.";

        public const string CourseCreateErrorMsg = "Could not create course.";
        public const string CourseCreateSuccessMsg = "Course created successfully.";
        public const string CourseDeleteErrorMsg = "Could not delete course.";
        public const string CourseDeleteSuccessMsg = "Course deleted successfully.";
        public const string CourseUpdateErrorMsg = "Course was not updated.";
        public const string CourseUpdateSuccessMsg = "Course updated successfully.";
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

        public const string CurriculumCreateErrorMsg = "Could not create curriculum.";
        public const string CurriculumCreateSuccessMsg = "Curriculum created successfully.";
        public const string CurriculumDeleteErrorMsg = "Could not delete curriculum.";
        public const string CurriculumDeleteSuccessMsg = "Curriculum deleted successfully.";
        public const string CurriculumUpdateErrorMsg = "Curriculum was not updated.";
        public const string CurriculumUpdateSuccessMsg = "Curriculum updated successfully.";
        public const string CurriculumNotFoundMsg = "Curriculum not found.";
        public const string CurriculumContainsCourseAlreadyMsg = "Curriculum contains this course already.";
        public const string CurriculumDoesNotContainCourseMsg = "Course not found in this curriculum.";

        public const string CurriculumOrStudentErrorMsg = "Invalid curriculum or student.";

        public const string DiplomaCreateErrorMsg = "Could not issue diploma.";
        public const string DiplomaCreateSuccessMsg = "Diploma issued successfully.";
        public const string DiplomaDeleteErrorMsg = "Could not delete diploma.";
        public const string DiplomaDeleteSuccessMsg = "Diploma deleted successfully.";
        public const string DiplomaExistsAlreadyMsg = "Diploma has already been issued for this curriculum.";
        public const string DiplomaNotFoundMsg = "Diploma not found.";

        public const string Disable2faErrorMsg = "Cannot disable 2FA for user.";
        public const string ExternalLoginInfoErrorMsg = "Unable to load external login info.";

        public const string ExamAssessedMsg = "Exam was assessed successfully.";
        public const string ExamAssessmentErrorMsg = "Exam was not assessed.";
        public const string ExamDownloadUnauthorizedMsg = "Exam download not allowed.";
        public const string ExamNotFoundMsg = "Exam not found.";
        public const string ExamSubmittedMsg = "Exam file submitted successfully.";
        public const string ExamSubmitErrorMsg = "Could not submit exam file.";

        public const string FileFormatInvalidMsg = "File format should be {0}.";
        public const string FileSubmittionDateMsg = "File can only be submitted on exam date.";
        public const string FileNotSubmittedMsg = "No file was submitted.";
        public const string FileEmptyErrorMsg = "File cannot be empty.";
        public const string FileSizeInvalidMsg = "File size should not exceed {0} MB.";
        public const string FileInvalidMsg = "Invalid file size.";

        public const string FormErrorsMsg = "Review the form for errors.";

        public const string GradeInvalidMsg = "Invalid grade.";
        public const string GradeAverageCoursesMsg = "The average grade is calculated on the basis of the best course grade received even if no certificate was awarded.";
        public const string GradeAverageCertificatesMsg = "The average grade is calculated on the basis of the certificates awarded taking into account the best certificate grade per course.";

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

        public const string RoleCreateErrorMsg = "Role was not created.";
        public const string RoleCreateSuccessMsg = "Role created successfully.";
        public const string RoleDeleteErrorMsg = "Role was not deleted.";
        public const string RoleDeleteSuccessMsg = "Role deleted successfully.";
        public const string RoleExistsErrorMsg = "Role name already exists.";
        public const string RoleNameInvalidErrorMsg = "Invalid role name.";
        public const string RoleNotFoundErrorMsg = "Role not found.";

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
        public const string StudentNotEligibleForDiplomaMsg = "Student not eligible for diploma in this curriculum.";

        public const string SearchByArticleTitleOrContent = "Search by title or content";
        public const string SearchByCourseName = "Search by course name";
        public const string SearchByUserName = "Search by name";

        public const string NotTrainerForCourseMsg = "Not authorized as trainer for this course.";
        public const string TrainerNotFoundMsg = "Trainer not found.";
        public const string TrainersEvaluateExamsAfterCourseEndsMsg = "Trainers can evaluate students' course performance once the course is over but not later than one month after the course has ended.";

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
