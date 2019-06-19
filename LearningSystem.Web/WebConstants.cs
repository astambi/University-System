namespace LearningSystem.Web
{
    public class WebConstants
    {
        // Roles
        public const string AdministratorRole = "Administrator";
        public const string BlogAuthorRole = "BlogAuthor";
        public const string TrainerRole = "Trainer";
        // Student Role => default role for registered users

        // Admin
        public const string AdminUsername = "admin";
        public const string AdminEmail = "admin@admin.com";

        // Areas
        public const string AdminArea = "Admin";
        public const string BlogArea = "Blog";

        // Secret Manager Keys
        public const string AdminPassword = "AdminPassword";
        public const string AuthFacebookAppId = "Authentication:Facebook:AppId";
        public const string AuthFacebookAppSecret = "Authentication:Facebook:AppSecret";
        public const string AuthGoogleClientId = "Authentication:Google:ClientId";
        public const string AuthGoogleClientSecret = "Authentication:Google:ClientSecret";

        // TempData Keys
        public const string TempDataSuccessMessageKey = "SuccessMessage";
        public const string TempDataErrorMessageKey = "ErrorMessage";
        public const string TempDataInfoMessageKey = "InfoMessage";

        // Project
        public const string HomeController = "Home";
        public const string CoursesController = "Courses";
        public const string UsersController = "Users";

        //Style
        public const string PrimaryStyle = "primary";
        public const string CreateStyle = "success";
        public const string EditStyle = "warning";
        public const string DeleteStyle = "danger";

        // Notifications
        public const string InvalidIdentityOrRoleMsg = "Invalid identity or role.";
        public const string InvalidUserMsg = "Invalid user.";
        public const string CourseNotFoundMsg = "Course not found.";
        public const string CourseCreatedMsg = "Course created successfully.";
        public const string CourseUpdatedMsg = "Course updated successfully.";
        public const string UserAddedToRoleMsg = "User {0} added to role {1}.";
        public const string UserRemovedFromRoleMsg = "User {0} removed from role {1}.";
    }
}
