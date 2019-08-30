namespace University.Web.Infrastructure.Helpers
{
    using System;

    public class PaginationHelpers
    {
        public static int GetTotalPages(int itemsCount, int pageSize = WebConstants.PageSize)
            => Math.Max(1, (int)Math.Ceiling(itemsCount / (double)pageSize));

        public static int GetValidCurrentPage(int currentPage, int totalPages)
            => currentPage < 1
            ? 1
            : currentPage > totalPages
                ? totalPages
                : currentPage;
    }
}
