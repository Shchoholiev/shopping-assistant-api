namespace ShoppingAssistantApi.Application.Paging;

public class PagedList<T>
{
    public IEnumerable<T> Items { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalPages { get; set; }

    public int TotalItems { get; set; }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public PagedList() { }

    public PagedList(IEnumerable<T> items, int pageNumber, int pageSize, int totalItems)
    {
        this.PageNumber = pageNumber;
        this.PageSize = pageSize;
        this.TotalItems = totalItems;
        this.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        this.Items = items;
    }
}