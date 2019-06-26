namespace LearningSystem.Web.Models
{
    public class SearchViewModel
    {
        public FormActionEnum Action { get; set; } = FormActionEnum.Search;

        public string SearchTerm { get; set; }

        public string Placeholder { get; set; }
    }
}
