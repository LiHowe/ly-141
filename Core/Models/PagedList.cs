namespace Core.Models;

public class PagedList<T>
{
    public List<T> Data { get; }
    public int TotalCount { get; }
    public int CurrentPage { get; }
    public int PageSize { get; }

    public PagedList(List<T> data, int total, int currentPage, int size)
    {
        Data = data;
        TotalCount = total;
        CurrentPage = currentPage;
        PageSize = size;
    }
}