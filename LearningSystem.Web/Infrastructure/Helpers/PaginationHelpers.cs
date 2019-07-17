namespace LearningSystem.Web.Infrastructure.Helpers
{
    using System;

    public class PaginationHelpers
    {
        public static int GetTotalPages(int itemsCount, int pageSize = WebConstants.PageSize)
            => Math.Max(1, (int)Math.Ceiling(itemsCount / (double)pageSize));

        public static int GetValidCurrentPage(int currentPage, int totalPages)
        {
            if (currentPage < 1)
            {
                currentPage = 1;
            }

            if (currentPage > totalPages)
            {
                currentPage = totalPages;
            }

            return currentPage;
        }
    }
}
