namespace GarageManagementSystem.Core.Extensions
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Apply pagination to IQueryable
        /// </summary>
        public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Limit max page size

            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Get total count for pagination
        /// </summary>
        public static async Task<int> GetTotalCountAsync<T>(this IQueryable<T> query)
        {
            return await Task.Run(() => query.Count());
        }
    }
}
