using Microsoft.EntityFrameworkCore;

namespace Repositories.Helpers
{
    /// <summary>
    /// Represents a paginated list of items along with pagination metadata.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    public class PagedList<T> : List<T>
    {
        /// <summary>
        /// Gets the pagination metadata.
        /// </summary>
        public MetaData MetaData { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        /// </summary>
        /// <param name="items">The list of items for the current page.</param>
        /// <param name="count">The total number of items.</param>
        /// <param name="pageNumber">The current page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than 0.");
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");

            MetaData = new MetaData
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = count,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize)
            };

            AddRange(items);
        }

        /// <summary>
        /// Creates a paginated list asynchronously from an IQueryable source.
        /// </summary>
        /// <param name="query">The source query.</param>
        /// <param name="pageNumber">The current page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the paginated list.</returns>
        public static async Task<PagedList<T>> ToPagedListAsync(IQueryable<T> query, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than 0.");
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");

            var count = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
