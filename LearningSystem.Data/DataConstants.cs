namespace LearningSystem.Data
{
    public class DataConstants
    {
        public const string StringMaxLength = "The {0} must be at max {1} characters long.";
        public const string StringMinMaxLength = "The {0} must be at least {2} and at max {1} characters long.";

        public const int ArticleTitleMinLength = 3;
        public const int ArticleTitleMaxLength = 50;
        public const int ArticleContentMinLength = 3;
        public const int ArticleContentMaxLength = 5000;
        public const string ArticlePublishDate = "The article Publish date must precede current date.";

        public const int CourseNameMaxLength = 50;
        public const int CourseDescriptionMaxLength = 2000;
        public const string CourseEndDate = "The Course End date cannot precede Start date.";

        public const int UserNameMinLength = 2;
        public const int UserNameMaxLength = 100;
        public const int UserUsernameMaxLength = 50;
        public const string UserBirthdate = "The user Birthdate must precede current date.";
    }
}
