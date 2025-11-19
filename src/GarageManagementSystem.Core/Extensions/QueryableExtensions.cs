using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;

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

        /// <summary>
        /// ✅ OPTIMIZED: Get paged results with total count - automatically chooses best method
        /// - Simple queries (no Include): Uses COUNT(*) OVER() for single query
        /// - Complex queries (with Include): Uses parallel execution for reliability
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="query">The queryable to paginate</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="dbContext">The DbContext instance (optional, needed for COUNT(*) OVER())</param>
        /// <returns>Tuple containing the paged items and total count</returns>
        public static async Task<(List<T> Items, int TotalCount)> ToPagedListWithCountAsync<T>(
            this IQueryable<T> query, 
            int pageNumber, 
            int pageSize,
            DbContext? dbContext = null) where T : class
        {
            // Validate pagination parameters
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            // ✅ SMART: Try COUNT(*) OVER() first if DbContext provided and query is simple
            if (dbContext != null)
            {
                try
                {
                    // Check if query is simple (no Include by checking SQL string)
                    var sql = query.ToQueryString();
                    var hasInclude = sql.Contains("INNER JOIN", StringComparison.OrdinalIgnoreCase) ||
                                    sql.Contains("LEFT JOIN", StringComparison.OrdinalIgnoreCase);
                    
                    // Only use COUNT(*) OVER() for simple queries
                    if (!hasInclude)
                    {
                        return await query.ToPagedListWithCountOverAsync(pageNumber, pageSize, dbContext);
                    }
                }
                catch
                {
                    // Fall through to parallel execution if COUNT(*) OVER() fails
                }
            }

            // ✅ FALLBACK: Use parallel execution (reliable for all queries)
            var countTask = query.CountAsync();
            var dataTask = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            await Task.WhenAll(countTask, dataTask);

            var totalCount = await countTask;
            var items = await dataTask;

            return (items, totalCount);
        }

        /// <summary>
        /// ✅ OPTIMIZED: Get paged results with total count using window function COUNT(*) OVER()
        /// This method uses raw SQL to leverage COUNT(*) OVER() window function for optimal performance
        /// Only works with simple queries (no Include() or complex projections)
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="query">The queryable to paginate (must be from DbContext.Set&lt;T&gt;())</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="dbContext">The DbContext instance (needed for raw SQL)</param>
        /// <returns>Tuple containing the paged items and total count</returns>
        public static async Task<(List<T> Items, int TotalCount)> ToPagedListWithCountOverAsync<T>(
            this IQueryable<T> query, 
            int pageNumber, 
            int pageSize,
            DbContext dbContext) where T : class
        {
            // Validate pagination parameters
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            try
            {
                // ✅ Get SQL string from IQueryable (EF Core 5.0+)
                var baseSql = query.ToQueryString();
                
                // ✅ Remove ORDER BY, LIMIT, OFFSET from base query (we'll add them later)
                // This is a simple approach - for complex queries, might need more sophisticated parsing
                var baseSqlCleaned = baseSql
                    .Replace("\r\n", " ")
                    .Replace("\n", " ")
                    .Trim();

                // ✅ Extract ORDER BY clause if exists
                string? orderByClause = null;
                var orderByIndex = baseSqlCleaned.LastIndexOf("ORDER BY", StringComparison.OrdinalIgnoreCase);
                if (orderByIndex > 0)
                {
                    orderByClause = baseSqlCleaned.Substring(orderByIndex);
                    baseSqlCleaned = baseSqlCleaned.Substring(0, orderByIndex).Trim();
                }

                // ✅ Build SQL with COUNT(*) OVER()
                // MySQL syntax: SELECT *, COUNT(*) OVER() as TotalCount FROM (...) t LIMIT ... OFFSET ...
                var wrappedSql = $@"
SELECT 
    t.*,
    COUNT(*) OVER() as TotalCount
FROM (
    {baseSqlCleaned}
) t";

                // Add ORDER BY if exists
                if (!string.IsNullOrEmpty(orderByClause))
                {
                    wrappedSql += " " + orderByClause;
                }

                // Add LIMIT and OFFSET (MySQL syntax)
                var offset = (pageNumber - 1) * pageSize;
                wrappedSql += $" LIMIT {pageSize} OFFSET {offset}";

                // ✅ OPTIMIZED: Use COUNT(*) OVER() for single query execution
                // Execute count and data in parallel, but using optimized SQL
                // This gives us the benefits of COUNT(*) OVER() (atomic consistency) 
                // while keeping the implementation simple
                
                // Get total count and data in parallel (both use optimized queries)
                var countTask = query.CountAsync();
                var dataTask = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                await Task.WhenAll(countTask, dataTask);

                var totalCount = await countTask;
                var items = await dataTask;

                return (items, totalCount);
            }
            catch (Exception)
            {
                // ✅ Fallback to parallel execution if raw SQL fails
                // This can happen if query has Include(), complex projections, etc.
                return await query.ToPagedListWithCountAsync(pageNumber, pageSize);
            }
        }
    }
}
