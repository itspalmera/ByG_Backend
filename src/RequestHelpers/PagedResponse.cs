namespace ByG_Backend.src.RequestHelpers
{
    public class PagedResponse<T>(List<T> items, int totalItems, int pageNumber, int pageSize)
    {
        public List<T> Items { get; set; } = items;
        public int PageNumber { get; set; } = pageNumber;
        public int PageSize { get; set; } = pageSize;
        public int TotalItems { get; set; } = totalItems;
        public int TotalPages => (int)Math.Ceiling(totalItems / (double)pageSize);
    }
}