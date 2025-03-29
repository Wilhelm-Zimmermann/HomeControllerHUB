namespace HomeControllerHUB.Domain.Models;

public class PaginatedList<T>
{
    public List<T> Items { get; }

    public int PageNumber { get; }

    public int TotalPages { get; }

    public int TotalCount { get; }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        var totalPage = (int)Math.Ceiling(count / (double)pageSize);

        PageNumber = pageNumber;
        TotalPages = totalPage > 0 ? totalPage : 1;
        TotalCount = count;
        Items = items;
    }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginatedList<TItem> Empty<TItem>(int pageNumber, int pageSize)
    {
        return new PaginatedList<TItem>(new List<TItem>(), 0, pageNumber, pageSize);
    }
}