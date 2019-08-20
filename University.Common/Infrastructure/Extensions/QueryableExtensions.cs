namespace University.Common.Infrastructure.Extensions
{
    using System;
    using System.Linq;

    public static class QueryableExtensions
    {
        public static IQueryable<TModel> GetPageItems<TModel>(this IQueryable<TModel> queryable,
            int page,
            int pageSize)
            where TModel : class
        {
            page = Math.Max(1, page); // min page
            pageSize = Math.Max(1, pageSize); // min pageSize

            return queryable
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }
    }
}
