using System.Text.Json;
using API.Helpers;
using Microsoft.AspNetCore.Http;

namespace API.Extensions
{
    public static class HttpExtensions
    {
        private const string Pagination = "Pagination";
        private const string AccessControlExposeHeaders = "Access-Control-Expose-Headers";

        public static void AddPaginationHeader(this HttpResponse response,
                                                       int currentPage,
                                                       int itemsPerPage,
                                                       int totalItems,
                                                       int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);

            var JsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            response.Headers.Add(Pagination, JsonSerializer.Serialize(paginationHeader, JsonOptions));
            response.Headers.Add(AccessControlExposeHeaders, Pagination);
        }
    }
}