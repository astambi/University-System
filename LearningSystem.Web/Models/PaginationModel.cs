namespace LearningSystem.Web.Models
{
    public class PaginationModel
    {
        public string Action { get; set; } = WebConstants.Index;

        public string SearchTerm { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int PreviousPage
            => this.CurrentPage == 1
            ? 1
            : this.CurrentPage - 1;

        public int NextPage
            => this.CurrentPage == this.TotalPages
            ? this.TotalPages
            : this.CurrentPage + 1;
    }
}
