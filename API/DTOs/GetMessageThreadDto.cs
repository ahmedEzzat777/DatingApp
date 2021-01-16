namespace API.DTOs
{
    public class GetMessageThreadDto
    {
        public string RecipientUsername { get; set; }
        public int pageNumber { get; set; }
        public int PageSize { get; set; }
    }
}