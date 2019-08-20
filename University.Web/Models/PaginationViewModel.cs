namespace University.Web.Models
{
    using University.Web.Infrastructure.Helpers;

    public class PaginationViewModel
    {
        public string Action { get; set; } = WebConstants.Index;

        public string SearchTerm { get; set; }

        public int RequestedPage { get; set; }

        public int TotalItems { get; set; }

        public int PageSize { get; set; } = WebConstants.PageSize;

        public int TotalPages
            => PaginationHelpers.GetTotalPages(this.TotalItems, this.PageSize);

        public int CurrentPage
            => PaginationHelpers.GetValidCurrentPage(this.RequestedPage, this.TotalPages);

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
