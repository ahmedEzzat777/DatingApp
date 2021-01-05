namespace API.Helpers
{
    public class PaginationHeader
    {

        public int CurrentPage { get; private set; }
        public int ItemsPerPage { get; private set; }
        public int TotalItems { get; private set; }
        public int TotalPages { get; private set; }

        public PaginationHeader(PaginationProperties paginationProperties)
        {
            CurrentPage = paginationProperties.CurrentPage;
            ItemsPerPage = paginationProperties.PageSize;
            TotalItems = paginationProperties.TotalCount;
            TotalPages = paginationProperties.TotalPages;
        }
    }
}