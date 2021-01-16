using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
    public class PagedList<T>: List<T>
    {
        public PaginationProperties PaginationProperties {get; private set;} = new PaginationProperties();

        public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            PaginationProperties.CurrentPage = pageNumber;
            PaginationProperties.TotalPages = (int)Math.Ceiling(count/(double)pageSize);
            PaginationProperties.PageSize = pageSize;
            PaginationProperties.TotalCount = count;
            AddRange(items);
        }

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1)*pageSize).Take(pageSize).ToListAsync();

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }

        public static async Task<PagedList<T>> CreateFromBottomAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var skipCount = count-pageSize*pageNumber;
            var items = await source.Skip(skipCount > 0? skipCount: count).Take(pageSize).ToListAsync();

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}