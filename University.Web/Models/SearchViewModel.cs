namespace University.Web.Models
{
    public class SearchViewModel
    {
        //public FormActionEnum Action { get; set; } = FormActionEnum.Search;

        public string Controller { get; set; }

        public string Action { get; set; }

        public string SearchTerm { get; set; }

        public string Placeholder { get; set; }
    }
}
